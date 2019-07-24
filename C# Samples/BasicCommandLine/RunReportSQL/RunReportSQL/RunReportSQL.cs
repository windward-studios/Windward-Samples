using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;

namespace RunReportSQL {

    class RunReportSQL {

        static void Main(string[] args) {

            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/Microsoft SQL Server - Template.docx");
            FileStream output = File.Create("../../../Samples/SQL Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            string strConn = "Data Source=mssql.windward.net;Initial Catalog=Northwind;User ID=demo;Password=demo";

            // SQL data source
            using (AdoDataSourceImpl adoDatasource = new AdoDataSourceImpl("System.Data.SqlClient", strConn)) {
                //run the report process
                myReport.ProcessSetup();
                //the second parameter is the name of the data source
                myReport.ProcessData(adoDatasource, "MSSQL");
                myReport.ProcessComplete();
            }

            //close out of our template file and output
            output.Close();
            template.Close();

            // Open the finished report
            string fullPath = Path.GetFullPath("../../../Samples/SQL Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }

    }

}