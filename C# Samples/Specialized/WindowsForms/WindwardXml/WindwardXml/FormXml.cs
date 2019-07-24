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

namespace WindwardXml
{
    public partial class FormXml : Form
    {
        public FormXml()
        {
            InitializeComponent();
        }

        public void DB2Report()
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/Windward Trucking 2 - Template.docx");
            FileStream output = File.Create("../../../Samples/Xml Report.pdf");

            // Create report process
            Report myReport = new ReportPdf(template, output);


            // Open an inputfilestream for our data file
            FileStream Xml = File.OpenRead("../../../Samples/Windward Trucking 2 - Data.xml");

            // Open a data object to connect to our xml file
            IReportDataSource data = new XmlDataSourceImpl(Xml, false);

            // Run the report process
            myReport.ProcessSetup();
            // The second parameter is "" to tell the process that our data is the default data source
            myReport.ProcessData(data, "");
            myReport.ProcessComplete();

            // Close out of our template file and output
            output.Close();
            template.Close();
            Xml.Close();

            // Opens the finished report
            string fullPath = Path.GetFullPath("../../../Samples/Xml Report.pdf");
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
