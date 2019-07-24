using System;
using System.Web.UI;

public partial class ReportForm : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		if (IsPostBack)
			return;
    }

	protected void btnSubmit_Click(object sender, EventArgs e)
	{

		// save what was selected
		Session["var"] = 1;
		Session["report"] = 0;

		// we use Response.Redirect instead of Server.Transfer because we want the report url for the final report.
		Response.BufferOutput = true;
		Response.Redirect("DisplayReport.aspx");
	}
}
