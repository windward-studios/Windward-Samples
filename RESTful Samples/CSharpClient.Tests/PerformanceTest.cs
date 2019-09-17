using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using RESTfulEngine.CSharpClient;
using System.IO;
using System.Threading;

namespace CSharpClient.Tests
{
    [TestClass]
    public class PerformanceTest
    {
        private static string samplesFolder = @"..\..\..\SampleTemplates";
		private static Uri serviceUri = new Uri(TestClient.uriRestEngine);

		[TestMethod]
        public void SynchronousRun()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(string.Format(@"{0}\Manufacturing.xml", samplesFolder)))}
            };

            var templatePath = string.Format(@"{0}\Manufacturing.docx", samplesFolder);

            var tempPath = Path.GetTempPath();
            var outputPath = string.Format(@"{0}SynchronousOutput.pdf", tempPath);

            using (var templateStream = File.OpenRead(templatePath))
            {
                using (var outputStream = File.Create(outputPath))
                {
                    var report = new ReportPdf(serviceUri, templateStream, outputStream);
                    report.Process(dataSources);
                }
            }
        }

        [TestMethod]
        public void AsynchronousRun()
        {
            var dataSources = new Dictionary<string, DataSource>()
            {
                {"MANF_DATA_2009", new XmlDataSource(File.OpenRead(string.Format(@"{0}\Manufacturing.xml", samplesFolder)))}
            };

            var templatePath = string.Format(@"{0}\Manufacturing.docx", samplesFolder);
            using (var templateStream = File.OpenRead(templatePath))
            {
                var report = new ReportPdf(serviceUri, templateStream);
                report.Process(dataSources);

                while (report.GetStatus() != Report.Status.Ready)
                    Thread.Sleep(1);
            }
        }
    }
}
