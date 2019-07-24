using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Kailua.net.windward.utils;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardInterfaces.net.windward.api.csharp;
using WindwardReportsAPI.net.windward.api.csharp;


namespace RunReportOData
{
    class RunReportODataSharepoint
    {
        static void Main(string[] args)
        {
            //Initialize the engine
            Report.Init();

            //Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/Sharepoint with OData - Template.docx");
            FileStream output = File.Create("../../../Samples/Sharepoint Report.pdf");

            //Create report process
            Report myReport = new ReportPdf(template, output);

            //Run the report process
            myReport.ProcessSetup();

            //Datasource connection code for 'ODataSample' (datasource name inside template)
            IReportDataSource ODataSampleData = new ODataDataSourceImpl("http://sharepointdemo.windward.net/_vti_bin/listdata.svc/", "windwarddemo\\demo", "demo", FileUtils.CONNECT_MODE.CREDENTIALS, 2);
            var dataSources = new Dictionary<string, IReportDataSource>() {
                {"sp", ODataSampleData}
            };
            myReport.ProcessData(dataSources);
            myReport.ProcessComplete();

            //Close out of our template file and output
            output.Close();
            template.Close();

            //Open the finished report
            string fullPath = Path.GetFullPath("../../../Samples/Sharepoint Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }
    }
}
