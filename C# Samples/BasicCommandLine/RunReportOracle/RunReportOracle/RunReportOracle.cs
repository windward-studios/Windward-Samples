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

namespace RunReportOracle
{
    class RunReportOracle
    {
        static void Main(string[] args)
        {

			// if connector is not installed, tell user
			if (!IsOracleDotNetConnectorInstalled)
				throw new ApplicationException("Please install the Oracle ADO.NET connector to run this example. Details at http://rpt.me/OracleConnector");

            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/Oracle - Template.docx");
            FileStream output = File.Create("../../../Samples/Oracle Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            // Oracle data source
            string strConn = "Data Source=oracle.windward.net:1521;Persist Security Info=True;User ID=HR;Password=HR;";
			// you can also use the "Oracle.ManagedDataAccess.Client" connector if installed (.NET 4.0 or later only)
            IReportDataSource data = new AdoDataSourceImpl("Oracle.DataAccess.Client", strConn);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "ORACLE");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            // Open the finished report
            string fullPath = Path.GetFullPath("../../../Samples/Oracle Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }

		#region test for connector

		/// <summary>
		/// Returns true if Oracle connector is installed.
		/// </summary>
		public static bool IsOracleDotNetConnectorInstalled
		{
			get
			{
				try
				{
					DataTable providers = DbProviderFactories.GetFactoryClasses();
					foreach (DataRow row in providers.Rows)
					{
						string providerClass = ((string)row[2]).ToLower();
						if (providerClass.StartsWith("oracle.manageddataaccess.client") || providerClass.StartsWith("oracle.dataaccess.client"))
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
