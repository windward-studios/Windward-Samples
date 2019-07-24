using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;

namespace RunReportAccess
{
    class RunReportAccess
    {
        static void Main(string[] args){

			// if connector is not installed, tell user
			if (! IsAccessDotNetConnectorInstalled)
				throw new ApplicationException("Please install the Access ADO.NET connector to run this example. Details at http://rpt.me/AccessConnector");

            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/Microsoft Access Datasource Connection - Template.docx");
            FileStream output = File.Create("../../../Samples/Access Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            // The data is stored in the Samples folder
            string fullPathData = Path.GetFullPath("../../../Samples/Northwind - Data.mdb");

            // Access data source
            string strConn = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq="+fullPathData;
            IReportDataSource data = new AdoDataSourceImpl("System.Data.Odbc", strConn);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "NWMINIACCESS");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            // Open the finished report
            string fullPath = Path.GetFullPath("../../../Samples/Access Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }

		#region test for connector

		/// <summary>
		/// Returns true if Access connector is installed.
		/// </summary>
		public static bool IsAccessDotNetConnectorInstalled
		{
			get
			{
				try
				{
					DbProviderFactories.GetFactory("System.Data.Odbc");
					if (HasAccessProviders("SOFTWARE\\ODBC\\ODBCINST.INI\\ODBC Drivers\\"))
						return true;
					if (IntPtr.Size == 4)
						if (HasAccessProviders("SOFTWARE\\Wow6432Node\\ODBC\\ODBCINST.INI\\ODBC Drivers\\"))
							return true;
					return false;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		private static bool HasAccessProviders(string key)
		{

			using (RegistryKey keyCLSID = Registry.LocalMachine.OpenSubKey(key, false))
			{
				if (keyCLSID == null)
					return false;

				string[] drivers = keyCLSID.GetValueNames();
				return drivers.Any(drvr => drvr.ToLower().IndexOf("*.accdb") != -1);
			}
		}

		#endregion
	}
}
