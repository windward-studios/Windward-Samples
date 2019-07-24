/*
 * Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
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

            xml.Root.Add(new XElement("Timeout", Timeout));

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

            if (report == null)
                xml.Root.Add(new XElement("Async", true));

            if (Datasets != null)
            {
                XElement datasets = new XElement("Datasets");

                foreach (var dataset in Datasets)
                    datasets.Add(dataset.GetXml());

                xml.Root.Add(datasets);
            }

            XDocument result;
            var status = Client.Post(new Uri(string.Format("{0}v1/reports", baseUri)), xml, Timeout, out result);
            if (status == HttpStatusCode.OK)
            {
                if (report != null)
                    ReadReport(result);
                else
                    ReadGuid(result);
            }
            else
                throw new ReportException(result.ToString());
        }

        private void ReadReport(XDocument result)
        {
            var bytes = System.Convert.FromBase64String(result.Descendants("Data").First().Value);
            Utils.WriteAllBytes(report, bytes);
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
            var encodedTemplate = System.Convert.ToBase64String(templateBytes);
            var xml = new XDocument(
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
    }
}
