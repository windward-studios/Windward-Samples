using System;
using System.Web.UI;

public partial class ReportForm : Page
{
	/// <summary>
	/// Which report format was selected.
	/// </summary>
	public int ReportNumber
	{
		get
		{
			if (rbPdf.Checked)
				return 0;
            if (rbDocx.Checked)
                return 1;
            if (rbCss.Checked)
				return 2;
			return -1;
		}
	}

    protected void Page_Load(object sender, EventArgs e)
    {
		if (IsPostBack)
			return;

        listVar.Items.Add("1 - Maria Anders, 5/1/2009");
        listVar.Items.Add("2 - Frederique Citeaux, 4/12/2009");
        listVar.Items.Add("3 - Maria Anders, 6/29/2009");
        listVar.Items.Add("4 - Laurence Lebihan, 6/1/2009");
        listVar.Items.Add("5 - Christina Berglund, 8/12/2009");
        listVar.Items.Add("6 - Thomas Hardy, 7/1/2009");
    }
	protected void btnSubmit_Click(object sender, EventArgs e)
	{

		// save what was selected
		Session["var"] = listVar.SelectedIndex + 1;
		Session["report"] = ReportNumber;

		// we use Response.Redirect instead of Server.Transfer because we want the report url for the final report.
		Response.BufferOutput = true;
		Response.Redirect("DisplayReport.aspx");
	}
}
