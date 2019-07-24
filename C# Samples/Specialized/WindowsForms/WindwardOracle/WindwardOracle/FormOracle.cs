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

namespace WindwardOracle
{
    public partial class FormOracle : Form
    {
        public FormOracle()
        {
            InitializeComponent();
        }

        public void OracleReport()
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/Oracle - Template.docx");
            FileStream output = File.Create("../../../Samples/Oracle Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);


            // Oracle data source
            string strConn = "Data Source=oracle.windward.net:1521;Persist Security Info=True;User ID=HR;Password=HR;";
            IReportDataSource data = new AdoDataSourceImpl("Oracle.DataAccess.Client", strConn);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "ORACLE");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            string fullPath = Path.GetFullPath("../../../Samples/Oracle Report.pdf");
            System.Diagnostics.Process.Start(fullPath);

        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            generateButton.Enabled = false;   // Disable button while report is being generated
            OracleReport();
            generateButton.Enabled = true;
        }
    }
}
