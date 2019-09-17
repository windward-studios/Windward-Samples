/*
 * Copyright (c) 2015-2017 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// The base class for a different report types.
    /// </summary>
    public abstract class Report
    {
        private Uri baseUri;
        private Stream template;
        private Stream report;
        // Image report fills in the list of pages.
        private List<byte[]> pages;
        private string guid;

        protected Report(Uri baseUri, Stream template, Stream report)
        {
            ctor(baseUri);
            this.template = template;
            this.report = report;
        }

        protected Report(Uri baseUri, Stream template)
        {
            ctor(baseUri);
            this.template = template;
        }

        // For the image report.
        protected Report(Uri baseUri, Stream template, List<byte[]> pages)
        {
            ctor(baseUri);
            this.template = template;
            this.pages = pages;
        }

        private static Uri FixUri(Uri uri)
        {
            string path = uri.AbsoluteUri;
            if (!path.EndsWith("/"))
                path += "/";
            return new Uri(path);
        }

        private void ctor(Uri baseUri)
        {
            this.baseUri = FixUri(baseUri);

            // Set up default values
            Timeout = 0;
            Hyphenate = Hyphenation.Template;
            TrackImports = false;
            RemoveUnusedFormats = true;
            CopyMetadata = CopyMetadataOption.IfNoDatasource;
            TrackErrors = 0;
            Errors = new List<Issue>();
            FirstPagePrinter = null;
            PrinterJobName = null;
            PrintCopies = 1;
            PrintDuplex = DuplexMode.Simplex;
            Dpi = 72;
        }

        public static Version GetVersion(Uri baseUri)
        {
            var uri = new Uri(string.Format("{0}v1/version", FixUri(baseUri)));

            XDocument response;
            var status = Client.Get(uri, System.Threading.Timeout.Infinite, out response);
            if (status == HttpStatusCode.OK)
            {
                return new Version { ServiceVersion = response.Descendants("ServiceVersion").First().Value, EngineVersion = response.Descendants("EngineVersion").First().Value };
            }
            else
                return null;
        }

        /// <summary>
        /// Generates a report from the template. The report is written to a stream passed in during a construction of this instance.
        /// </summary>
        /// <param name="dataSources">A list of data sources to be applied to the report being generated.</param>
        public virtual void Process(IDictionary<string, DataSource> dataSources)
        {
            var xml = CreateXmlDocument();

            ApplyDatasources(xml, dataSources);

            Process(xml);
        }

        /// <summary>
        /// Generates a report from the template. The report is written to a stream passed in during a construction of this instance.
        /// This method doesn't apply any data sources.
        /// </summary>
        public virtual void Process()
        {
            var xml = CreateXmlDocument();

            Process(xml);
        }

        /// <summary>
        /// Retrieves asynchronously generated report.
        /// </summary>
        public byte[] GetReport()
        {
            var uri = new Uri(string.Format("{0}v1/reports/{1}", baseUri, guid));

            XDocument response;
            var status = Client.Get(uri, Timeout, out response);
            if (status == HttpStatusCode.OK)
            {
                return System.Convert.FromBase64String(response.Descendants("Data").First().Value);
            }
            return null;
        }

        /// <summary>
        /// Deletes previously generated report.
        /// </summary>
        public void Delete()
        {
            var uri = new Uri(string.Format("{0}v1/reports/{1}", baseUri, guid));
            XDocument response;
            var status = Client.Delete(uri, Timeout, out response);
        }

        public enum Status
        {
            Ready,
            Working,
            Error,
            NotFound
        }

        public Status GetStatus()
        {
            var uri = new Uri(string.Format("{0}v1/reports/{1}/status", baseUri, guid));

            XDocument response;
            var status = Client.Get(uri, Timeout, out response);
            if (status == HttpStatusCode.OK)
            {
                return Status.Ready;
            }
            else if (status == HttpStatusCode.Accepted)
            {
                return Status.Working;
            }
            else if (status == HttpStatusCode.InternalServerError)
            {
                return Status.Error;
            }
            else
                return Status.NotFound;
        }

        protected void Process(XDocument xml)
        {
            SetReportOption(xml, "Description", Description);
            SetReportOption(xml, "Title", Title);
            SetReportOption(xml, "Subject", Subject);
            SetReportOption(xml, "Keywords", Keywords);
            SetReportOption(xml, "Locale", Locale);
            SetReportOption(xml, "MainPrinter", MainPrinter);
            SetReportOption(xml, "FirstPagePrinter", FirstPagePrinter);
            SetReportOption(xml, "PrinterJobName", PrinterJobName);

            xml.Root.Add(new XElement("Timeout", Timeout));

            xml.Root.Add(new XElement("PrintCopies", PrintCopies));

            switch (PrintDuplex)
            {
                case DuplexMode.Simplex:
                    xml.Root.Add(new XElement("PrintDuplex", "simplex"));
                    break;
                case DuplexMode.Horizontal:
                    xml.Root.Add(new XElement("PrintDuplex", "horizontal"));
                    break;
                case DuplexMode.Vertical:
                    xml.Root.Add(new XElement("PrintDuplex", "vertical"));
                    break;
            }

            switch (Hyphenate)
            {
                case Hyphenation.On:
                    xml.Root.Add(new XElement("Hyphenate", "on"));
                    break;
                case Hyphenation.Off:
                    xml.Root.Add(new XElement("Hyphenate", "off"));
                    break;
                case Hyphenation.Template:
                    xml.Root.Add(new XElement("Hyphenate", "template"));
                    break;
            }

            xml.Root.Add(new XElement("TrackImports", TrackImports));
            xml.Root.Add(new XElement("RemoveUnusedFormats", RemoveUnusedFormats));
            xml.Root.Add(new XElement("TrackErrors", TrackErrors));
            
            switch (CopyMetadata)
            {
                case CopyMetadataOption.IfNoDatasource:
                    xml.Root.Add(new XElement("CopyMetadata", "nodatasource"));
                    break;
                case CopyMetadataOption.Never:
                    xml.Root.Add(new XElement("CopyMetadata", "never"));
                    break;
                case CopyMetadataOption.Always:
                    xml.Root.Add(new XElement("CopyMetadata", "always"));
                    break;
            }

            if (report == null && pages == null)
                xml.Root.Add(new XElement("Async", true));

            if (Datasets != null)
            {
                XElement datasets = new XElement("Datasets");

                foreach (var dataset in Datasets)
                    datasets.Add(dataset.GetXml());

                xml.Root.Add(datasets);
            }

            xml.Root.Add(new XElement("Dpi", Dpi));

            XDocument result;
            var status = Client.Post(new Uri(string.Format("{0}v1/reports", baseUri)), xml, Timeout, out result);
            if (status == HttpStatusCode.OK)
            {
                if (report != null)
                    ReadReport(result);
                else if (pages != null)
                    ReadPages(result);
                else
                    ReadGuid(result);
            }
            else
                throw new ReportException(result.ToString());
        }

        private void ReadReport(XDocument result)
        {
            var bytes = Convert.FromBase64String(result.Descendants("Data").First().Value);
            Utils.WriteAllBytes(report, bytes);

            foreach (var error in result.Descendants("Errors").Elements())
            {
                Errors.Add(new Issue() { Message = error.Descendants("Message").First().Value });
            }
        }

        private void ReadPages(XDocument result)
        {
            var xmlPages = result.Descendants("base64Binary");
            foreach (var xmlPage in xmlPages)
            {
                var bytes = Convert.FromBase64String(xmlPage.Value);
                pages.Add(bytes);
            }
        }

        private void ReadGuid(XDocument result)
        {
            guid = result.Descendants("Guid").First().Value;
        }

        private void SetReportOption(XDocument xml, string name, string option)
        {
            if (option != null)
                xml.Root.Add(new XElement(name, option));
        }

        private XDocument CreateXmlDocument()
        {
            var templateBytes = Utils.ReadAllBytes(template);
            var encodedTemplate = Convert.ToBase64String(templateBytes);
            var xml = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Template",
                    new XElement("Data", encodedTemplate),
                    new XElement("OutputFormat", OutputFormat)
                    )
                );
            return xml;
        }

        private void ApplyDatasources(XDocument xml, IDictionary<string, DataSource> dataSources)
        {
            if (dataSources.Count > 0)
            {
                var xmlDatasources = new XElement("Datasources");

                foreach (var entry in dataSources)
                {
                    xmlDatasources.Add(entry.Value.GetXml(entry.Key));
                }
                xml.Root.Add(xmlDatasources);
            }
        }

        protected abstract string OutputFormat
        {
            get;
        }

        /// <summary>
        /// Desired DPI for bitmap image reports.  Default is 72.
        /// </summary>
        public int Dpi { get; set; }

        public string Description { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Keywords { get; set; }
        public string Locale { get; set; }

        public int Timeout { get; set; }

        public enum Hyphenation
        {
            On,
            Off,
            Template
        }

        public Hyphenation Hyphenate { get; set; }

        public bool TrackImports { get; set; }
        public bool RemoveUnusedFormats { get; set; }

        /// <summary>
        /// Enable or disable the error handling and verify functionality.
        /// Available options are:
        /// 0 - error handling and verify is disabled.  This is the default.
        /// 1 - enable error handling.
        /// 2 - enable verify.
        /// 3 - enable both error handling and verify.
        /// Any other value is ignored and disables the functionality.
        /// </summary>
        public int TrackErrors { get; set; }

        /// <summary>
        /// Contains a list of issues (errors and warnings) found during the report generation.
        /// The list is populating only if the error handling and verify is enabled.
        /// </summary>
        public IList<Issue> Errors { get; private set; }

        /// <summary>
        /// Options for the CopyMetadata property.
        /// </summary>
        public enum CopyMetadataOption
        {
            /// <summary>
            /// Copy the Windward metadata to the output report if no datasources were applied to the report. This is the default.
            /// </summary>
            IfNoDatasource,

            /// <summary>
            /// Never copy the Windward metadata to the output report.
            /// </summary>
            Never,

            /// <summary>
            /// Always copy the Windward metadata to the output report.
            /// </summary>
            Always
        }

        /// <summary>
		/// Get/set if the Windward metadata will be copied to the generated report. This can only occur if the template and generated report are both OpenXML files. The default is IfNoDatasource.
        /// </summary>
        public CopyMetadataOption CopyMetadata { get; set; }

        /// <summary>
        /// Defines a list of datasets.
        /// </summary>
        public Dataset[] Datasets { get; set; }

        /// <summary>
        /// Name of a printer to send the output to.
        /// </summary>
        public string MainPrinter
        {
            get;
            set;
        }

        /// <summary>
        /// Printer name for the first page of the report.
        /// </summary>
        public string FirstPagePrinter
        {
            get;
            set;
        }

        /// <summary>
        /// Printer job name.
        /// </summary>
        public string PrinterJobName
        {
            get;
            set;
        }

        /// <summary>
        /// Number of copies to print.
        /// </summary>
        public int PrintCopies
        {
            get;
            set;
        }

        /// <summary>
        /// Printer duplex mode constants.
        /// </summary>
        public enum DuplexMode
        {
            /// <summary>
            /// One-sided printing.  This is the default.
            /// </summary>
            Simplex,

            /// <summary>
            /// Two-sided printing.  Prints on both sides of the paper for portrait output.
            /// </summary>
            Horizontal,

            /// <summary>
            /// Two-sided printing.  Prints on both sides of the paper for landscape output.
            /// </summary>
            Vertical
        }

        /// <summary>
        /// Printer duplex mode.
        /// </summary>
        public DuplexMode PrintDuplex
        {
            get;
            set;
        }
    }
}
