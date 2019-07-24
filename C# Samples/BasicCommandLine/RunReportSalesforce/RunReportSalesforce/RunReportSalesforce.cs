using net.windward.api.csharp;
using System.IO;
using WindwardReportsAPI.net.windward.api.csharp;

namespace RunReportSalesforce
{
    class RunReportSalesforce
    {
        static void Main(string[] args)
        {

            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/template.docx");
            FileStream output = File.Create("../../../Samples/Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            // Salesforce data source
            SFDataSourceImpl data = new SFDataSourceImpl(@"demo@windward.net", @"w1ndw@rd", "BtqoH7pIR6rkR0fwh1YU156Hp", true);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "sfdemo");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            // Open the finished report
            string fullPath = Path.GetFullPath("../../../Samples/Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }

	}
}