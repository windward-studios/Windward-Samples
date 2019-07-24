using System.Collections.Generic;
using net.windward.api.csharp;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;

namespace VariableExample
{
    class VariableExample
    {
        static void Main(string[] args)
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/Variable Invoice Sample - Template.docx");
            FileStream output = File.Create("../../../Samples/Variable Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);


            // SQL data source
            string strConn = "Data Source=mssql.windwardreports.com;Initial Catalog=Northwind;User ID=demo;Password=demo;";
            IReportDataSource data = new AdoDataSourceImpl("System.Data.SqlClient", strConn);

            //run the report process
            myReport.ProcessSetup();

            //This is where we pass in the parameters
			Dictionary<string, object> map = new Dictionary<string, object>();
            //order is our variable
            map.Add("order", 10537);
            //This is the function where we actually tell our report the parameter values
            myReport.Parameters = map;

            //the second parameter is the name of the data source
            myReport.ProcessData(data, "MSSQL");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            // Open the finished report
            string fullPath = Path.GetFullPath("../../../Samples/Variable Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }
    }
}
