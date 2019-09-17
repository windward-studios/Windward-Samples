/*
 * Copyright (c) 2017 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTfulEngine.CSharpClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace CSharpClient.Tests
{
    [TestClass]
    public class TestClient
    {
        /// <summary>
        /// Base URI of the hosted engine.
        /// </summary>
        public const string uriRestEngine = "http://localhost:58097";
        // public const string uriRestEngine = "http://localhost:81";

        private Uri uri = new Uri(uriRestEngine);

		private string SampleTemplatesFolder = @"..\..\..\SampleTemplates";

        [TestMethod]
        public void Client_GetVersion()
        {
            RESTfulEngine.CSharpClient.Version v = Report.GetVersion(uri);
            Assert.IsNotNull(v);
            Assert.IsTrue(v.EngineVersion is string);
            Assert.IsTrue(v.ServiceVersion is string);
        }

        [TestMethod]
        public void Client_PostTemplateReturnsReportPdf()
        {
            using (var templateFile = File.OpenRead(SampleTemplatesFolder + @"\Sample1.docx"))
            {
                using (var output = new MemoryStream())
                {
                    var report = new ReportPdf(uri, templateFile, output);
                    report.Process();
                    Assert.IsTrue(IsPdf(output));
                }
            }
        }

        [TestMethod]
        public void Client_PostTemplateWithXmlData()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(SampleTemplatesFolder + @"\Manufacturing.xml"))}
            };

            var templateFilePath = SampleTemplatesFolder + @"\Manufacturing.docx";
            var outputFilePath = SampleTemplatesFolder + @"\XmlDataOutput.pdf";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportPdf(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        [TestMethod]
        public void Client_GeneratePostScript()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(SampleTemplatesFolder + @"\Manufacturing.xml"))}
            };

            var templateFilePath = SampleTemplatesFolder + @"\Manufacturing.docx";
            var outputFilePath = SampleTemplatesFolder + @"\XmlDataOutput.ps";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportPostScript(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        // [TestMethod]
        // This is a manual test.  Change the printer name in the ReportPrinter() constructor
        // and run the test when needed.
        public void Client_PrinterOutput()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(SampleTemplatesFolder + @"\Manufacturing.xml"))}
            };

            var templateFilePath = SampleTemplatesFolder + @"\Manufacturing.docx";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                var report = new ReportPrinter(uri, templateFile, "Brother DCP-1610W series Printer");
                report.Process(dataSources);
            }
        }

        [TestMethod]
        public void Client_PostTemplateWithAdoData()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MSSQL", new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo")}
            };

            var templateFilePath = SampleTemplatesFolder + @"\MsSqlTemplate.docx";
            var outputFilePath = SampleTemplatesFolder + @"\AdoDataOutput.pdf";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportPdf(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        [TestMethod]
        public void Client_VariablesTest()
        {
            var ds = new XmlDataSource(File.OpenRead(SampleTemplatesFolder + @"\Manufacturing.xml"));
            ds.Variables = new List<TemplateVariable>()
            {
                new TemplateVariable() { Name = "Var1", Value = "hi there" }
            };

            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", ds}
            };

            var templateFilePath = SampleTemplatesFolder + @"\Variables.docx";
            var outputFilePath = SampleTemplatesFolder + @"\VariablesOutput.pdf";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportPdf(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        [TestMethod]
        public void Client_PostTemplateAsync()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(SampleTemplatesFolder + @"\Manufacturing.xml"))}
            };

            var templateFilePath = SampleTemplatesFolder + @"\Manufacturing.docx";
            var outputFilePath = SampleTemplatesFolder + @"\AsyncOutput.pdf";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                var report = new ReportPdf(uri, templateFile);
                report.Process(dataSources);

                while (report.GetStatus() == Report.Status.Working)
                    Thread.Sleep(100);

                if (report.GetStatus() == Report.Status.Ready)
                {
                    File.WriteAllBytes(outputFilePath, report.GetReport());

                    report.Delete();
                }
            }
        }

        [TestMethod]
        public void Client_PostJsonSample()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new JsonDataSource(new Uri("http://json.windward.net/Northwind.json"), "demo", "demo")}
            };

            var templateFilePath = SampleTemplatesFolder + @"\JsonSample.docx";
            var outputFilePath = SampleTemplatesFolder + @"\JsonOutput.docx";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportDocx(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        [TestMethod]
        public void Client_PostODataSample()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new ODataDataSource(new Uri("http://odata.windward.net/Northwind/Northwind.svc"), 2)}
            };

            var templateFilePath = SampleTemplatesFolder + @"\ODataSample.docx";
            var outputFilePath = SampleTemplatesFolder + @"\ODataOutput.docx";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportDocx(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        [TestMethod]
        public void Client_XmlDataSourceCanUseConnectionString()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"XPATH2", new XmlDataSource("Url=http://windwardwebsitestorage.blob.core.windows.net/devtestfiles/web-based-templates/XML/SouthWind.xml;")}
            };
            WebClient web = new WebClient();
            using (var template = web.OpenRead("http://windwardwebsitestorage.blob.core.windows.net/devtestfiles/web-based-templates/XML/XML.docx"))
            {
                using (var output = new MemoryStream())
                {
                    var report = new ReportPdf(uri, template, output);
                    report.Process(dataSources);
                    Assert.IsTrue(IsPdf(output));
                }
            }
        }

        [TestMethod]
        public void Client_ODataDataSourceCanUseConnectionString()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new ODataDataSource("Url=http://odata.windward.net/Northwind/Northwind.svc;Version=2")}
            };

            var templatePath = SampleTemplatesFolder + @"\ODataSample.docx";
            using (var template = File.OpenRead(templatePath))
            {
                using (var output = new MemoryStream())
                {
                    var report = new ReportPdf(uri, template, output);
                    report.Process(dataSources);
                    Assert.IsTrue(IsPdf(output));
                }
            }
        }

        [TestMethod]
        public void Client_PostSalesforceSample()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new SalesforceDataSource("demo@windward.net", "w1ndw@rd", "BtqoH7pIR6rkR0fwh1YU156Hp")}
            };

            var templateFilePath = SampleTemplatesFolder + @"\SalesforceSample.docx";
            var outputFilePath = SampleTemplatesFolder + @"\SalesforceOutput.docx";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportDocx(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        [TestMethod]
        public void Client_TestDatasets()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=AdventureWorks;User=demo;Password=demo")}
            };

            var templateFilePath = SampleTemplatesFolder + @"\DataSet.docx";
            var outputFilePath = SampleTemplatesFolder + @"\DataSetOutput.pdf";
            var datasetFilePath = SampleTemplatesFolder + @"\DataSet.rdlx";
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    using (var datasetFile = File.OpenRead(datasetFilePath))
                    {
                        var report = new ReportPdf(uri, templateFile, outputFile);

                        report.Datasets = new Dataset[] { new Dataset(datasetFile) };

                        report.Process(dataSources);
                    }
                }
            }
        }

        [TestMethod]
        public void Client_PostWithTrackErrorsEnabledReturnsListOfErrors()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new XmlDataSource("Url=http://windwardwebsitestorage.blob.core.windows.net/devtestfiles/web-based-templates/XML/SouthWind.xml;")}
            };
            WebClient web = new WebClient();
            using (var template = web.OpenRead("http://windwardwebsitestorage.blob.core.windows.net/devtestfiles/web-based-templates/XML/XML.docx"))
            {
                using (var output = new MemoryStream())
                {
                    var report = new ReportDocx(uri, template, output);
                    report.TrackErrors = 3;
                    report.Process(dataSources);
                    Assert.AreEqual(3, report.Errors.Count);
                }
            }
        }

        /// <summary>
        /// Test generating an image report.
        /// </summary>
        [TestMethod]
        public void Client_TestImageReport()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(SampleTemplatesFolder + @"\Manufacturing.xml"))}
            };

            var templateFilePath = SampleTemplatesFolder + @"\Manufacturing.docx";
            List<byte[]> pages = new List<byte[]>();
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                var imageFormat = "bmp";

                var report = new ReportImage(uri, imageFormat, templateFile, pages);
                report.Dpi = 96;

                report.Process(dataSources);

                var pageNo = 0;
                foreach (var page in pages)
                {
                    var path = string.Format(@"{0}\image_{1}.{2}", SampleTemplatesFolder, ++pageNo, imageFormat);
                    File.WriteAllBytes(path, page);
                }
            }
        }

        private static bool IsPdf(MemoryStream data)
        {
            // Compare the first 5 bytes to the '%PDF-' literal.
            byte[] buffer = new byte[5];
            Array.Copy(data.GetBuffer(), buffer, 5);
            return Enumerable.SequenceEqual(buffer, new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2d });
        }
    }
}
