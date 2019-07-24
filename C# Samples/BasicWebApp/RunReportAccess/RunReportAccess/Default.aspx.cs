using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Win32;
using net.windward.api.csharp;
using System.IO;

namespace BasicWindwardEngine
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

			// if connector is not installed, tell user
			if (!IsAccessDotNetConnectorInstalled)
				throw new ApplicationException("Please install the Access ADO.NET connector to run this example. Details at http://rpt.me/AccessConnector");
		}

        protected void btnRunReport_Click(object sender, EventArgs e)
        {
            // DisplayReport.aspx will generate the report.
            Response.Redirect("DisplayReport.aspx");
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