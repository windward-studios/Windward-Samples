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

namespace WindwardMySQL
{
    public partial class FormMySQL : Form
    {
        public FormMySQL()
        {
            InitializeComponent();
        }

        public void MySQLReport()
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/MySQL - Template.docx");
            FileStream output = File.Create("../../../Samples/MySQL Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);


            // MySQL data source
            string strConn = "server=mysql.windward.net;database=sakila;user id=test;password=test;";
            IReportDataSource data = new AdoDataSourceImpl("MySql.Data.MySqlClient", strConn);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "MYSQL");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            string fullPath = Path.GetFullPath("../../../Samples/MySQL Report.pdf");
            System.Diagnostics.Process.Start(fullPath);

        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            generateButton.Enabled = false;   // Disable button while report is being generated
            MySQLReport();
            generateButton.Enabled = true;
        }
    }
}
