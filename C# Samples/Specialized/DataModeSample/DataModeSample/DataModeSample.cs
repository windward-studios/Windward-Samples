using net.windward.api.csharp;
using System;
using System.IO;
using WindwardInterfaces.net.windward.api.csharp;

namespace DataModeSample
{
    internal class DataModeSample
    {
        static void Main(string[] args)
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/DataModeTemplate.docx");
            FileStream output = File.Create("../../../Output/Output.docx");

            // Create report process
            Report report = new ReportDocx(template, output);

            // Create data file stream and link it to the report data stream
            FileStream dataFileStream = File.Create("../../../Output/Output_Data.xml");
            report.DataStream = dataFileStream;

            /**
             * The different data mode options (uncomment one of the following options)
             * ---------------------------------------------------------------------------
             */
            // Sets the data file to contain the returned data from the tags in the DataModeTemplate
            report.DataMode = Report.DATA_MODE.DATA;

            // Sets the data file to contain the select attributes from the tags in the DataModeTemplate
            //report.DataMode = Report.DATA_MODE.SELECT;

            // Sets the data file to contain all attributes from the tags in the DataModeTemplate
            //report.DataMode = Report.DATA_MODE.ALL_ATTRIBUTES;

            // Sets the data file to contain the data (uuencoded) from bitmap tags in the DataModeTemplate
            //report.DataMode = Report.DATA_MODE.INCLUDE_BITMAPS;

            // Sets the data file to contain all the information from the tags and data in the DataModeTemplate
            //report.DataMode = Report.DATA_MODE.DATA | Report.DATA_MODE.SELECT | Report.DATA_MODE.ALL_ATTRIBUTES | Report.DATA_MODE.INCLUDE_BITMAPS;

            // Embeds the data file within the DOCX file and sets the data file to contain the data from the tags in the DataModeTemplate
            //report.DataMode = Report.DATA_MODE.EMBED | Report.DATA_MODE.DATA;
            /**
             * ---------------------------------------------------------------------------
             */

            // Run the report process
            report.ProcessSetup();

            // Open a data object to connect to our xml file and then process the data
            string datasourceUrl = Path.GetFullPath("../../../Samples/DataSource.xml");
            IReportDataSource data = new SaxonDataSourceImpl(string.Format("Url={0}", datasourceUrl), null);
            report.ProcessData(data, "SW");

            // Finish the report process
            report.ProcessComplete();

            // Close out of our template file and output
            data.Close();
            output.Close();
            template.Close();

            // All finished!
            Console.WriteLine("Finished generating report: " + output.Name);
            Console.WriteLine("Finished generating data file: " + output.Name);
            Console.ReadKey();
        }
    }
}
