using System;
using System.Collections.Generic;
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

        }

        protected void btnRunReport_Click(object sender, EventArgs e)
        {
            // DisplayReport.aspx will generate the report.
            Response.Redirect("DisplayReport.aspx");
        }


    }
}