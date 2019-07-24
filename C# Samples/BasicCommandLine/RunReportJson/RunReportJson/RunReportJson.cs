using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using System.IO;
using WindwardReportsAPI.net.windward.api.csharp;
using WindwardInterfaces.net.windward.api.csharp;

namespace RunReportJson
{
    class RunReportJson
    {
        static void Main(string[] args)
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/JSON - Template.docx");
            FileStream output = File.Create("../../../Samples/Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            // Connect to our JSON database
            IReportDataSource data = new JsonDataSourceImpl("http://json.windward.net/Northwind.json", JsonDataSourceImpl.MODE.CONNECTION_STRING);

            // Run the report process
            myReport.ProcessSetup();
            // The second parameter is "" to tell the process that our data is the default data source
            myReport.ProcessData(data, "");
            myReport.ProcessComplete();

            // Close out of our template file and output
            output.Close();
            template.Close();

            // Opens the finished report
            string fullPath = Path.GetFullPath("../../../Samples/Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }
    }
}
