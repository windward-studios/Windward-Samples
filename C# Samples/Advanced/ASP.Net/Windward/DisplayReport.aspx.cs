using System;
using System.Web.UI;
using System.IO;
using System.Collections.Generic;

using net.windward.api.csharp;
using WindwardInterfaces.net.windward.api.csharp;

public partial class DisplayReport : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

		Stream template = null;
		Stream data = null;
		Report proc;

		try
		{
			// get the report files
            string templateFile = Request.PhysicalApplicationPath + "files\\Example_Template.docx";
			template = new FileStream(templateFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			// create the report
			string contentType;
			switch ((int)Session["report"])
			{
				case 0:
					contentType = "application/pdf";
					proc = new ReportPdf(template);
					break;
                case 1:
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    proc = new ReportDocx(template);
                    break;
				case 2:
					contentType = "text/html";
					proc = new ReportHtml(template);
					((ReportHtml)proc).SetImagePath(Request.PhysicalApplicationPath + "images", "images", "wr_");
					break;
				default:
					lblResult.Text = "Error: unknown report type " + Session["report"];
					return;
			}
			proc.ProcessSetup();

            // set variables
            Dictionary<string, object> map = new Dictionary<string, object>();
            map.Add("LeaveRequestId", Session["var"]);
		    map.Add("CSRName", "John Brown");

            // apply data
            data = new FileStream(Request.PhysicalApplicationPath + "files\\Example_Data.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		    IReportDataSource ds = new XmlDataSourceImpl(data, false);
		    proc.Parameters = map;
            proc.ProcessData(ds, "");
            ds.Close();

			proc.ProcessComplete();

			// get the output and display it
			Response.ContentType = contentType;
			Response.BinaryWrite(((MemoryStream)proc.GetReport()).ToArray());
			proc.Close();
		}

		catch (Exception ex)
		{
			lblResult.Text = ex.Message;
			return;
		}
		// close everything
		finally
		{
			if (template != null)
				template.Close();
			if (data != null)
				data.Close();
		}

		// this will throw an exception so we have it here at the end - must be last line of code!
		Response.End();
	}
}
