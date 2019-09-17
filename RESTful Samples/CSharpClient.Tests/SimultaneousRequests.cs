using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using RESTfulEngine.CSharpClient;
using System.IO;
using System.Threading;
using System.Linq;

namespace CSharpClient.Tests
{
    [TestClass]
    public class SimultaneousRequests
    {
        [TestMethod]
        public void TestMultipleRequests()
        {
            IList<Thread> requests = new List<Thread>();

            for (int i = 0; i < 2; ++i)
            {
                requests.Add(new Thread(new ThreadStart(Request1)));
                requests.Add(new Thread(new ThreadStart(Request2)));
                requests.Add(new Thread(new ThreadStart(Request3)));
                requests.Add(new Thread(new ThreadStart(Request4)));
                requests.Add(new Thread(new ThreadStart(Request5)));
                requests.Add(new Thread(new ThreadStart(Request6)));
                requests.Add(new Thread(new ThreadStart(Request7)));
                requests.Add(new Thread(new ThreadStart(Request8)));
                requests.Add(new Thread(new ThreadStart(Request9)));
            }


            foreach (Thread t in requests)
                t.Start();

            foreach (Thread t in requests)
                t.Join();
        }

		private static Uri uri = new Uri(TestClient.uriRestEngine);
        private static string SampleTemplatesFolder = @"..\..\..\SampleTemplates";
        private static Random rnd = new Random(17);

        public static void Request1()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=AdventureWorks;User=demo;Password=demo")}
            };

            var templateFilePath = SampleTemplatesFolder + @"\DataSet.docx";
            var outputFilePath = string.Format(@"{0}\DataSetOutput{1}.pdf", SampleTemplatesFolder, rnd.Next());
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

        public static void Request2()
        {
            RESTfulEngine.CSharpClient.Version v = Report.GetVersion(uri);
            Assert.IsNotNull(v);
            Assert.IsTrue(v.EngineVersion is string);
            Assert.IsTrue(v.ServiceVersion is string);
        }

        public static void Request3()
        {
            using (var template = File.OpenRead(SampleTemplatesFolder + @"\Sample1.docx"))
            {
                using (var output = new MemoryStream())
                {
                    var report = new ReportPdf(uri, template, output);
                    report.Process();

                    Assert.IsTrue(output.Length > 8);

					// Compare the first 7 bytes to the '%PDF-1.' literal.
					byte[] buffer = new byte[7];
					Array.Copy(output.GetBuffer(), buffer, 7);
					string hdr = System.Text.Encoding.UTF8.GetString(buffer);
					Assert.AreEqual("%PDF-1.", hdr);
                }
			}
        }

        public static void Request4()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(SampleTemplatesFolder + @"\Manufacturing.xml"))}
            };

            var templateFilePath = SampleTemplatesFolder + @"\Manufacturing.docx";
            var outputFilePath = string.Format(@"{0}\XmlDataOutput{1}.pdf", SampleTemplatesFolder, rnd.Next());
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportPdf(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        public static void Request5()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MSSQL", new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo")}
            };

            var templateFilePath = SampleTemplatesFolder + @"\MsSqlTemplate.docx";
            var outputFilePath = string.Format(@"{0}\AdoDataOutput{1}.pdf", SampleTemplatesFolder, rnd.Next());
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportPdf(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        public static void Request6()
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
            var outputFilePath = string.Format(@"{0}\VariablesOutput{1}.pdf", SampleTemplatesFolder, rnd.Next());
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportPdf(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        public static void Request7()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(SampleTemplatesFolder + @"\Manufacturing.xml"))}
            };

            var templateFilePath = SampleTemplatesFolder + @"\Manufacturing.docx";
            var outputFilePath = string.Format(@"{0}\AsyncOutput{1}.pdf", SampleTemplatesFolder, rnd.Next());
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

        public static void Request8()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new JsonDataSource(new Uri("http://json.windward.net/Northwind.json"), "demo", "demo")}
            };

            var templateFilePath = SampleTemplatesFolder + @"\JsonSample.docx";
            var outputFilePath = string.Format(@"{0}\JsonOutput{1}.docx", SampleTemplatesFolder, rnd.Next());
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportDocx(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

        public static void Request9()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"", new ODataDataSource(new Uri("http://odata.windward.net/Northwind/Northwind.svc"), 2)}
            };

            var templateFilePath = SampleTemplatesFolder + @"\ODataSample.docx";
            var outputFilePath = string.Format(@"{0}\ODataOutput{1}.docx", SampleTemplatesFolder, rnd.Next());
            using (var templateFile = File.OpenRead(templateFilePath))
            {
                using (var outputFile = File.Create(outputFilePath))
                {
                    var report = new ReportDocx(uri, templateFile, outputFile);
                    report.Process(dataSources);
                }
            }
        }

    }
}
