using System;
using System.Collections.Generic;
using net.windward.api.csharp;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;
using net.windward.xmlreport.errorhandling;


namespace ErrorHandlingVerify
{
    class Program
    {

        /**
         * Sample document demonstrating how to include Error Handling into a java app and print the error stream to a .txt file
         *
         * @author Adam Austin
         */
        static void Main(string[] args)
        {
            try {
                // Initialize the engine
                Report.Init();

                // To generate a report, first we need a Report object.  For now, we're using the
                // pdf format to output.
                FileStream template = File.OpenRead("../../../Samples/Smart Energy Template.docx");
                FileStream reportStream = File.Create("../../../Samples/report.pdf");
                Report report = new ReportPdf(template, reportStream);

                // Preparation...
                Console.Out.WriteLine("Generating report...");
                report.ProcessSetup();

                // Set Track Verify and Error Handling issues during report generation based off a command line argument
                string trackErrorSetting = "";
                if (args.Length > 0)
                {
                    trackErrorSetting = args[0];
                }

                switch (trackErrorSetting)
                {
                    case ("0"):
                        Console.Out.WriteLine("Track Errors: None");
                        report.TrackErrors = (int)Report.ERROR_HANDLING.NONE;
                        break;
                    case ("1"):
                        Console.Out.WriteLine("Track Errors: Error Handling");
                        report.TrackErrors = (int)Report.ERROR_HANDLING.TRACK_ERRORS;
                        break;
                    case ("2"):
                        Console.Out.WriteLine("Track Errors: Verify");
                        report.TrackErrors = (int)Report.ERROR_HANDLING.VERIFY;
                        break;
                    case ("3"):
                    default:
                        Console.Out.WriteLine("Track Errors: All");
                        report.TrackErrors = (int)Report.ERROR_HANDLING.ALL;
                        break;
                }

                // Set up the data hash map.
                var dataProviders = new Dictionary<string, IReportDataSource>();

                // Create an instance of DataSourceProvider
                System.Xml.XPath.XPathDocument Xml1 = new System.Xml.XPath.XPathDocument(File.OpenRead("../../../Samples/Smart Energy - Broken.xml"));
                IReportDataSource datasource = new XmlDataSourceImpl(Xml1);

                // Add the data source to the data hash map
                dataProviders.Add("", datasource);

                // Process the data stored in the hash map
                report.ProcessData(dataProviders);

                // And... DONE!
                report.ProcessComplete();
                reportStream.Close();
                template.Close();

                // Print errors found by Error Handling and Verify to the command line and the file "Issues.txt"
                ErrorInfo outputissues = report.GetErrorInfo();
                java.util.List errors = outputissues.getErrors();

                Console.Out.WriteLine();
                Console.Out.WriteLine("---------------------------------------------------");
                Console.Out.WriteLine("Errors found during Verify upon Report Generation:");
                Console.Out.WriteLine("---------------------------------------------------");

                using (System.IO.StreamWriter file = new System.IO.StreamWriter("../../../Samples/Issues.txt"))
                {
                    file.WriteLine("---------------------------------------------------");
                    file.WriteLine("Errors found by Verify upon Report Generation:");
                    file.WriteLine("---------------------------------------------------");

                    // Print every issue to the command line and the isseus.txt file
                    for (int i = 0; i < errors.size(); i++)
                    {
                        Console.Out.WriteLine(((Issue)errors.get(i)).getMessage());
                        file.WriteLine(((Issue)errors.get(i)).getMessage());
                    }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.StackTrace.ToString());
            }
            Console.Out.WriteLine("\n\nGeneration finished. Click \"Enter\" to dismiss window.");
            Console.In.Read();
        }
    }
}
