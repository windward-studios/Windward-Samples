using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardInterfaces.net.windward.api.csharp;

namespace WindwardSQL
{
    public partial class FormSQL : Form
    {
        public FormSQL()
        {
            InitializeComponent();
        }


        public void SQLReport()
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/Microsoft SQL Server - Template.docx");
            FileStream output = File.Create("../../../Samples/Sql Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);

            string strConn = "Data Source=mssql.windwardreports.com;Initial Catalog=Northwind;User ID=demo;Password=demo;";
            IReportDataSource data = new AdoDataSourceImpl("System.Data.SqlClient", strConn);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "MSSQL");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            string fullPath = Path.GetFullPath("../../../Samples/Sql Report.pdf");
            System.Diagnostics.Process.Start(fullPath);

        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            generateButton.Enabled = false;   // Disable button while report is being generated
            SQLReport();
            generateButton.Enabled = true;
        }
    }
}
