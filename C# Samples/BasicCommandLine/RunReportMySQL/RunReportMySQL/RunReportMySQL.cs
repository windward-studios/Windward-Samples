using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;

namespace RunReportMySQL
{
    class RunReportMySQL
    {
        static void Main(string[] args)
        {

			// if connector is not installed, tell user
			if (!IsMySqlDotNetConnectorInstalled)
				throw new ApplicationException("Please install the MySql ADO.NET connector to run this example. Details at http://rpt.me/MySqlConnector");

            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/MySQL - Template.docx");
            FileStream output = File.Create("../../../Samples/MySQL Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            // MySQL data source
            string strConn = "server=mysql.windward.net;database=sakila;user id=test;password=test;";
            IReportDataSource data = new AdoDataSourceImpl("MySql.Data.MySqlClient", strConn);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "MYSQL");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            // Open the finished report
            string fullPath = Path.GetFullPath("../../../Samples/MySQL Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }

		#region test for connector

		/// <summary>
		/// Returns true if MySql connector is installed.
		/// </summary>
		public static bool IsMySqlDotNetConnectorInstalled
		{
			get
			{
				try
				{
					DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		#endregion
	}
}
