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

namespace WindwardDB2
{
    public partial class FormDB2 : Form
    {
        public FormDB2()
        {
            InitializeComponent();
        }

        public void DB2Report()
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/DB2 - Templates.xlsx");
            FileStream output = File.Create("../../../Samples/DB2 Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);


            // DB2 data source
            string strConn = "server=db2.windward.net;database=Sample;User ID=demo;Password=demo;";
            IReportDataSource data = new AdoDataSourceImpl("IBM.Data.DB2", strConn);

            //run the report process
            myReport.ProcessSetup();
            //the second parameter is the name of the data source
            myReport.ProcessData(data, "DB2"); ;
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            string fullPath = Path.GetFullPath("../../../Samples/DB2 Report.pdf");
            System.Diagnostics.Process.Start(fullPath);

        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            generateButton.Enabled = false;   // Disable button while report is being generated
            DB2Report();
            generateButton.Enabled = true;
        }
    }
}
