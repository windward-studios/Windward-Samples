using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using net.windward.api.csharp;
using System.IO;

namespace BasicWindwardEngine
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

			// if connector is not installed, tell user
			if (!IsMySqlDotNetConnectorInstalled)
				throw new ApplicationException("Please install the MySql ADO.NET connector to run this example. Details at http://rpt.me/MySqlConnector");
		}

        protected void btnRunReport_Click(object sender, EventArgs e)
        {
            // DisplayReport.aspx will generate the report.
            Response.Redirect("DisplayReport.aspx");
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