﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using net.windward.api.csharp;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;

namespace BasicWindwardEngine
{
    public partial class DisplayReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string basePath = Request.PhysicalApplicationPath + "\\";

            // Initialize the engine. License and configuration settings in web.config.
            Report.Init();

            // Open template file
            FileStream template = File.OpenRead(basePath + "Windward Trucking 2 - Template.docx");

            // Create report process
            Report myReport = new ReportPdf(template);

            // Open data file
            FileStream Xml = File.OpenRead(basePath + "Windward Trucking 2 - Data.xml");

            // Make a data object to connect to our xml file
            IReportDataSource data = new SaxonDataSourceImpl(Xml);

            // Run the report process
            myReport.ProcessSetup();
            // The second parameter is "" to tell the process that our data is the unnamed data source
            myReport.ProcessData(data, "sax");
            myReport.ProcessComplete();

            // Close out of our template file
            template.Close();
            Xml.Close();

            // Opens the finished report
            //Response.ContentType = "application/pdf"; // this would have the pdf open in the browser (disable content-disposition:attachment if you want this)
            Response.ContentType = "application/save"; // this is used with content-disposition to give the proper name of the file
            Response.AppendHeader("content-disposition", "attachment; filename=\"Report.pdf\"");
            Response.BinaryWrite(((MemoryStream)myReport.GetReport()).ToArray());
            Response.End();   // Must be called for MS Office documents
        }
    }
}