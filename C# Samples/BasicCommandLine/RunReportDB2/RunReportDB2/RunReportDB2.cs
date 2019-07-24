using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;

namespace RunReportDB2
{
    class RunReportDb2
    {
        static void Main(string[] args)
        {

			// if connector is not installed, tell user
			if (!IsDb2DotNetConnectorInstalled)
				throw new ApplicationException("Please install the DB2 ADO.NET connector to run this example. Details at http://rpt.me/DB2Connector");

            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/DB2 - Templates.xlsx");
            FileStream output = File.Create("../../../Samples/DB2 Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            // DB2 data source
            string strConn = "server=db2.windward.net;database=Sample;User ID=demo;Password=demo;";
            IReportDataSource data = new AdoDataSourceImpl("IBM.Data.DB2", strConn);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "DB2");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            // Open the finished report
            string fullPath = Path.GetFullPath("../../../Samples/DB2 Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
		}

		#region test for connector

		/// <summary>
		/// Returns true if DB2 connector is installed.
		/// </summary>
		public static bool IsDb2DotNetConnectorInstalled
		{
			get
			{
				try
				{
					DataTable providers = DbProviderFactories.GetFactoryClasses();
					foreach (DataRow row in providers.Rows)
					{
						string providerClass = ((string)row[2]).ToLower();
						if (providerClass.StartsWith("ibm.data.db2"))
							return true;
					}
					return false;
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
