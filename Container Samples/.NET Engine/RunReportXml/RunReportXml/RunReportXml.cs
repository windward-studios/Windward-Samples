using net.windward.api.csharp;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;

namespace RunReportXml
{
    class RunReportXml
    {
        static void Main(string[] args)
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../Samples/Windward Trucking 2 - Template.docx");
            FileStream output = File.Create("../Samples/Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            // Open a data object to connect to our xml file
            string url = Path.GetFullPath("../Samples/Windward Trucking 2 - Data.xml");
            string xsd = null;
            IReportDataSource data = new SaxonDataSourceImpl(string.Format("Url={0}", url), xsd);

            // Run the report process
            myReport.ProcessSetup();

            // The second parameter is "sax" to tell the process that our data is named sax. This must match the name given to the datasource inside the template
            myReport.ProcessData(data, "sax");
            myReport.ProcessComplete();

            // Close the completed output, the source template file and data file
            data.Close();
            output.Close();
            template.Close();
        }
    }
}