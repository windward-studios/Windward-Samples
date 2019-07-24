using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTfulEngine.CSharpClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CSharpClient.Tests
{
    [TestClass]
    public class TestClient
    {
        private Uri uri = new Uri("http://localhost:49731/");

        private string SampleTemplatesFolder = @"..\..\..\samples";

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
            using (var template = File.OpenRead(SampleTemplatesFolder + @"\Sample1.docx"))
            {
                using (var output = new MemoryStream())
                {
                    var report = new ReportPdf(uri, template, output);
                    report.Process();

                    Assert.IsTrue(output.Length > 8);

                    // Compare the first 8 bytes to the '%PDF-1.5' literal.

                    byte[] buffer = new byte[8];
                    Array.Copy(output.GetBuffer(), buffer, 8);

                    Assert.IsTrue(Enumerable.SequenceEqual(buffer, new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2d, 0x31, 0x2e, 0x35 }));
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
    }
}
