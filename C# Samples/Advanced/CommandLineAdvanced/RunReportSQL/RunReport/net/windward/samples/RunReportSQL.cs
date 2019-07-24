/*
* Copyright (c) 2011 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

using System.Collections.Generic;
using System.Net;
using Kailua.net.windward.utils;
using log4net.Config;
using WindwardInterfaces.net.windward.api.csharp;
using WindwardInterfaces.net.windward.datasource;
using WindwardReportsAPI.net.windward.api.csharp;
using net.windward.api.csharp;
using System;
using System.IO;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardReportsDrivers.net.windward.datasource.ado;
using WindwardReportsDrivers.net.windward.datasource.xml;
using Exception = System.Exception;
using System.Collections;

namespace RunReportSQL.net.windward.samples {

    /// <summary>
    /// A sample usage of Windward Reports. This program generates reports based on the command line.
    /// This project is used for two purposes, as a sample and as a way to run reports easily from the command line (mostly for testing).
    /// The second ues provides one downside and one upside as a sample. The downside is it includes items like the recorder that would
    /// not be included if this was solely as a sample. The upside is it does pretty much everything because of the needs as a way to
    /// test any report.
    /// 
    /// To get the parameters, the RunReport with no parameters and it will list them all out.
    /// </summary>
    class RunReportSQL {
        /// <summary>
        /// Create a report using Windward Reports.
        /// </summary>
        /// <param name="args">run with no parameters to list out usage.</param>
        static void Main(string[] args) {

            // if no arguments, then we list out the usage.
            if (args.Length < 2) {
                DisplayUsage();
                return;
            }

            // the try here is so we can print out an exception if it is thrown. This code does minimal error checking and no other
            // exception handling to keep it simple & clear.
            try {
                // Initialize the reporting engine. This will throw an exception if the engine is not fully installed or you
                // do not have a valid license key in RunReport.exe.config.
                Report.Init();
                Console.Out.WriteLine("Running in {0}-bit mode", IntPtr.Size * 8);

                // parse the arguments passed in. This method makes no calls to Windward, it merely organizes the passed in arguments.
                CommandLine cmdLine = CommandLine.Factory(args);

                // This turns on log4net logging. You can also call log4net.Config.XmlConfigurator.Configure(); directly.
                // If you do not make this call, then there will be no logging (which you may want off).
                BasicConfigurator.Configure();

                // run one report
                if (!cmdLine.IsPerformance)
                    RunOneReport(cmdLine, args.Length == 2);
                else {
                    string dirReports = Path.GetDirectoryName(Path.GetFullPath(cmdLine.ReportFilename)) ?? "";
                    if (!Directory.Exists(dirReports)) {
                        Console.Error.WriteLine(string.Format("The directory {0} does not exist", dirReports));
                        return;
                    }

                    // drop out threads - twice the number of cores.
                    int numThreads = cmdLine.NumThreads;
                    numReportsRemaining = cmdLine.NumReports;
                    ReportWorker[] workers = new ReportWorker[numThreads];
                    for (int ind = 0; ind < numThreads; ind++)
                        workers[ind] = new ReportWorker(ind, cmdLine);
                    System.Threading.Thread[] threads = new System.Threading.Thread[numThreads];
                    for (int ind = 0; ind < numThreads; ind++)
                        threads[ind] = new System.Threading.Thread(workers[ind].DoWork);

                    DateTime startTime = DateTime.Now;
                    Console.Out.WriteLine(string.Format("Start time: {0}, {1} threads, {2} reports", startTime.ToLongTimeString(),
                                                        numThreads, cmdLine.NumReports));
                    Console.Out.WriteLine();
                    Console.Out.Write("[Thread number:Report number]; ");
                    for (int ind = 0; ind < numThreads; ind++)
                        threads[ind].Start();

                    // we wait
                    for (int ind = 0; ind < numThreads; ind++)
                        threads[ind].Join();
                    Console.Out.WriteLine();
                    Console.Out.WriteLine();
                    DateTime endTime = DateTime.Now;
                    TimeSpan elapsed = endTime - startTime;
                    Console.Out.WriteLine(string.Format("End time: {0}, Elapsed time: {1}, time per report: {2}",
                                                        endTime.ToLongTimeString(),
                                                        elapsed.ToString(), TimeSpan.FromTicks(elapsed.Ticks / cmdLine.NumReports)));
                }

                Report.Shutdown();
            }
            catch (Exception ex) {
                while (ex != null) {
                    Console.Error.WriteLine(string.Format("Error: {0}\n     stack: {1}\n", ex.Message, ex.StackTrace));
                    ex = ex.InnerException;
                }
                throw;
            }
        }

        private static int numReportsRemaining;

        private static bool HasNextReport {
            get {
                lock (typeof(RunReportSQL)) {
                    numReportsRemaining--;
                    return numReportsRemaining >= 0;
                }
            }
        }

        private class ReportWorker {
            private readonly int threadNum;
            private readonly CommandLine cmdLine;

            public ReportWorker(int threadNum, CommandLine cmdLine) {
                this.threadNum = threadNum;
                this.cmdLine = cmdLine;
            }

            public void DoWork() {
                string dirReports = Path.GetDirectoryName(Path.GetFullPath(cmdLine.ReportFilename)) ?? "";
                string extReport = cmdLine.ReportFilename.Substring(cmdLine.ReportFilename.LastIndexOf('.'));
                for (int rptNum = 0; HasNextReport; rptNum++) {
                    string reportFilename = Path.GetFullPath(Path.Combine(dirReports, Guid.NewGuid().ToString()) + extReport);
                    CommandLine cl = new CommandLine(cmdLine, reportFilename);
                    RunOneReport(cl, false);
                    Console.Out.Write(string.Format("[{0}:{1}]; ", threadNum, rptNum + 1));
                }
            }
        }

        private static void RunOneReport(CommandLine cmdLine, bool preservePodFraming) {

            DateTime startTime = DateTime.Now;

            // get the template and output file streams. Output is null for printers
            Stream template;
            if ((FileUtils.GetFilesystemType(cmdLine.TemplateFilename) & (FileUtils.FilenameType.drive | FileUtils.FilenameType.unc)) != 0)
                template = new FileStream(cmdLine.TemplateFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            else {
                WebRequest request = WebRequest.Create(cmdLine.TemplateFilename);
                template = request.GetResponse().GetResponseStream();
            }
            using (template) {
                Stream output;
                if ((cmdLine.ReportFilename.ToLower().EndsWith(".htm")
                        || cmdLine.ReportFilename.ToLower().EndsWith(".html")
                        || cmdLine.ReportFilename.ToLower().EndsWith(".xhtml"))
                    && ReportHtml.SplitPagesStatic)
                    output = null;
                else if (!cmdLine.ReportFilename.ToLower().EndsWith(".prn"))
                    output = new FileStream(cmdLine.ReportFilename, FileMode.Create, FileAccess.Write, FileShare.None);
                else
                    output = null;
                if (!cmdLine.IsPerformance) {
                    Console.Out.WriteLine(string.Format("Template: {0}", cmdLine.TemplateFilename));
                    Console.Out.WriteLine(string.Format("Report: {0}", cmdLine.ReportFilename));
                }

                // Create the report object, based on the file extension
                using (Report report = CreateReport(cmdLine, template, output)) {
                    if (cmdLine.BaseDirectory != null)
                        report.BaseDirectory = cmdLine.BaseDirectory;

                    // if we are applying no datasources then we keep the POD framing in the generated report.
                    if (preservePodFraming)
                        report.PreservePodFraming = true;
                    // if we have a locale, we set it (used when applying datasources).
                    if (cmdLine.Locale != null) {
                        if (!cmdLine.IsPerformance)
                            Console.Out.WriteLine(string.Format("Using locale: {0}", cmdLine.Locale));
                        report.Locale = cmdLine.Locale;
                    }
                    if (cmdLine.TemplateVersion != 0)
                        report.TemplateVersion = cmdLine.TemplateVersion;

                    // the data.xml file
                    Stream dataStream = null;
                    if (cmdLine.DataMode != 0) {
                        report.DataMode = cmdLine.DataMode;
                        if (cmdLine.DataFileName != null) {
                            dataStream = new FileStream(cmdLine.DataFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                            report.DataStream = dataStream;
                        }
                    }

                    // This first call parses the template and prepares the report so we can apply data to it.
                    report.ProcessSetup();

                    IDictionary<string, IReportDataSource> dataProviders = new Dictionary<string, IReportDataSource>();

                    // Now for each datasource, we apply it to the report. This is complex because it handles all datasource types
                    // as well as recording and playback.
                    foreach (CommandLine.DatasourceInfo dsInfo in cmdLine.Datasources) {

                        // build the datasource
                        IReportDataSource datasource;
                        Stream dsStream = null;
                        Stream schemaStream = null;
                        switch (dsInfo.Type) {
                            // An XML datasource.
                            case CommandLine.DatasourceInfo.TYPE.XML:
                                if (!cmdLine.IsPerformance) {
                                    if (string.IsNullOrEmpty(dsInfo.SchemaFilename))
                                        Console.Out.WriteLine(string.Format("XML datasource: {0}", dsInfo.Filename));
                                    else
                                        Console.Out.WriteLine(string.Format("XML datasource: {0}, schema {1}", dsInfo.Filename, dsInfo.SchemaFilename));
                                }

                                // http or ftp access - assume no schema
                                if (dsInfo.Filename.IndexOf(':') > 1) {
                                    WebClient myClient = new WebClient();
                                    if (dsInfo.Username != null && dsInfo.Password != null)
                                        myClient.Credentials = new NetworkCredential(dsInfo.Username, dsInfo.Password);
                                    datasource = new XmlDataSourceImpl(dsInfo.Filename, myClient.Credentials);
                                }

                                // Note: we have not (yet) implemented using username/password when opening local files
                                else {
                                    // just an XML file
                                    if (string.IsNullOrEmpty(dsInfo.SchemaFilename)) {
                                        dsStream = new FileStream(dsInfo.Filename, FileMode.Open, FileAccess.Read);
                                        datasource = new XmlDataSourceImpl(dsStream, false);
                                    }

                                    // XML file & schema file
                                    else {
                                        dsStream = new FileStream(dsInfo.Filename, FileMode.Open, FileAccess.Read);
                                        schemaStream = new FileStream(dsInfo.SchemaFilename, FileMode.Open, FileAccess.Read);
                                        datasource = new XmlDataSourceImpl(dsStream, schemaStream, false);
                                    }
                                }
                                break;

                            // An XML REST datasource.
                            case CommandLine.DatasourceInfo.TYPE.REST:
                                if (!cmdLine.IsPerformance)
                                    Console.Out.WriteLine(string.Format("XML datasource via REST: {0}", dsInfo.Filename));

                                // no schema supported (yet)
                                datasource = new XmlDataSourceImpl(dsInfo.Filename, DotNetDatasourceBase.CONNECT_MODE.REST, dsInfo.Username,
                                                                   dsInfo.Password);
                                break;

                            case CommandLine.DatasourceInfo.TYPE.JSON:
                                datasource = new JsonDataSourceImpl(dsInfo.Filename, JsonDataSourceImpl.MODE.CONNECTION_STRING);
                                break;


                            // An OData datasource.
                            case CommandLine.DatasourceInfo.TYPE.ODATA:
                                if (!cmdLine.IsPerformance)
                                    Console.Out.WriteLine(string.Format("OData datasource: {0}", dsInfo.Filename));

                                datasource = new ODataDataSourceImpl(dsInfo.Filename);
                                Console.WriteLine("datasource = " + datasource);
                                break;

                            // A SalesForce datasource.
                            case CommandLine.DatasourceInfo.TYPE.SFORCE:
                                if (!cmdLine.IsPerformance)
                                    Console.Out.WriteLine(string.Format("SalesForce datasource: {0}", dsInfo.Filename));
                                datasource = new SFDataSourceImpl(dsInfo.Username, dsInfo.Password, true);
                                break;

                            // An XML SharePoint datasource.
                            case CommandLine.DatasourceInfo.TYPE.SHAREPOINT:
                                if (!cmdLine.IsPerformance)
                                    Console.Out.WriteLine(string.Format("XML datasource via SharePoint: {0}", dsInfo.Filename));

                                // no schema supported (yet)
                                datasource = new XmlDataSourceImpl(dsInfo.Filename, DotNetDatasourceBase.CONNECT_MODE.SHAREPOINT, dsInfo.Username,
                                                                   dsInfo.Password);
                                break;

                            case CommandLine.DatasourceInfo.TYPE.SQL:
                                if (!cmdLine.IsPerformance)
                                    Console.Out.WriteLine(string.Format("{0} datasource: {1}", dsInfo.SqlDriverInfo.Name, dsInfo.ConnectionString));
                                datasource = new AdoDataSourceImpl(dsInfo.SqlDriverInfo.Classname, dsInfo.ConnectionString);
                                break;

                            default:
                                throw new ArgumentException(string.Format("Unknown datasource type {0}", dsInfo.Type));
                        }
                        if (!cmdLine.IsPerformance) {
                            if (!string.IsNullOrEmpty(dsInfo.Username))
                                Console.Out.WriteLine("    username={0}", dsInfo.Username);
                            if (!string.IsNullOrEmpty(dsInfo.Password))
                                Console.Out.WriteLine("    password={0}", dsInfo.Password);
                            if (dsInfo.PodFilename != null)
                                Console.Out.WriteLine("    POD filename={0}", dsInfo.PodFilename);
                        }

                        dataProviders.Add(dsInfo.Name, datasource);

                        // because of the switch above we have to explicitly close instead of structuring as a using{}
                        if (dsStream != null)
                            dsStream.Close();
                        if (schemaStream != null)
                            schemaStream.Close();
                        // datasource.Close();
                    }

                    // this applies the datasource to the report populating the tags.
                    if (dataProviders.Count > 0) {
						report.Parameters = cmdLine.Map;
                        report.ProcessData(dataProviders);
					}
                    if (!cmdLine.IsPerformance)
                        Console.Out.WriteLine("all data applied, generating report");

                    // Now that all the data has been applied, we generate the final output report. This does the
                    // page layout and then writes out the output file.
                    report.ProcessComplete();

                    // If it is an html report and has images, and embedded images option not chosen, we write these out
                    if (report is ReportHtml && !((ReportHtml)report).EmbedImages) {
                        Trap.trap(((ReportHtml)report).Bitmaps.Length > 0);
                        string dir = Path.GetDirectoryName(Path.GetFullPath(cmdLine.ReportFilename));
                        foreach (HtmlBitmap bitmap in ((ReportHtml)report).Bitmaps) {
                            string filename = Path.Combine(dir, bitmap.Filename);
                            if (!cmdLine.IsPerformance)
                                Console.Out.WriteLine(string.Format("Saving image {0}", filename));
                            using (FileStream bmFile = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                                bmFile.Write(bitmap.Filedata, 0, bitmap.Filedata.Length);
                            }
                        }
                    }

                    // close out data.xml
                    if (dataStream != null) {
                        dataStream.Close();
                        Console.Out.WriteLine("data written to: " + cmdLine.DataFileName);
                    }

                    // Close the output template to get everything written. (No "using (...)" because for printer output it is null.)
                    if (output != null)
                        output.Close();

                    // Close output streams for HTML -- don't worry if split_pages en/disabled
                    if (report is ReportHtml && ((ReportHtml)report).Pages != null) {
                        List<byte[]> pages = ((ReportHtml)report).Pages;
                        string prefix = Path.GetFileNameWithoutExtension(cmdLine.ReportFilename);
                        string directory = Path.GetDirectoryName(Path.GetFullPath(cmdLine.ReportFilename));
                        string extension = Path.GetExtension(cmdLine.ReportFilename);
                        for (int i = 0; i < pages.Count; i++) {
                            string filename = directory + "\\" + prefix + "_" + i + extension;
                            File.WriteAllBytes(filename, pages[i]);
                            Console.Out.WriteLine("HTML page written to " + filename);
                        }
                    }

                    if (!cmdLine.IsPerformance)
                        Console.Out.WriteLine(string.Format("{0} built, {1} pages long. Elapsed time: {2}",
                            cmdLine.ReportFilename, report.NumPages, DateTime.Now - startTime));

                    //					Console.Out.WriteLine("press any key to end.");
                    //					Console.In.Read();

                    if (cmdLine.Launch) {
                        Console.Out.WriteLine(string.Format("launching report {0}", cmdLine.ReportFilename));
                        System.Diagnostics.Process.Start(cmdLine.ReportFilename);
                    }
                }
            }
        }

        private static Report CreateReport(CommandLine cmdLine, Stream template, Stream output) {
            Report report;
            string ext = cmdLine.ReportFilename.Substring(cmdLine.ReportFilename.LastIndexOf('.') + 1).ToLower();
            switch (ext) {
                case "csv":
                    return new ReportCsv(template, output);
                case "docx":
                case "docm":
                    return new ReportDocx(template, output);
                case "htm":
                    report = new ReportHtml(template, output);
                    ((ReportHtml)report).CssType = ReportHtml.CSS.NO;
                    return report;
                case "html":
                    report = new ReportHtml(template, output);
                    ((ReportHtml)report).CssType = ReportHtml.CSS.INCLUDE;
                    return report;
                case "pdf":
                    return new ReportPdf(template, output);
                case "pptx":
                case "pptm":
                    return new ReportPptx(template, output);
                case "rtf":
                    return new ReportRtf(template, output);
                case "txt":
                    return new ReportText(template, output);
                case "xhtml":
                    report = new ReportHtml(template, output);
                    ((ReportHtml)report).CssType = ReportHtml.CSS.INCLUDE;
                    ((ReportHtml)report).Spec = ReportHtml.BROWSER.XHTML;
                    return report;
                case "xlsx":
                case "xlsm":
                    return new ReportXlsx(template, output);
                case "prn":
                    string printer = cmdLine.ReportFilename.Substring(0, cmdLine.ReportFilename.Length - 4);
                    if (string.IsNullOrEmpty(printer))
                        throw new ArgumentException("no printer specified");
                    return new ReportPrinter(template, printer);
                default:
                    throw new ArgumentException(string.Format("Unknown extension {0}", ext));
            }
        }

        private static void DisplayUsage() {
            Console.Out.WriteLine("Windward Reports version " + Report.Version);
            Console.Out.WriteLine("usage: RunReport template_file output_file [-basedir path] [-xml xml_file | -sql connection_string | -sforce | -oracle connection_string | -ole oledb_connection_string] [key=value | ...]");
            Console.Out.WriteLine("       The template file can be a rtf, xml (WordML), docx, pptx, or xlsx file.");
            Console.Out.WriteLine("       The output file extension determines the report type created:");
            Console.Out.WriteLine("           output.csv - SpreadSheet CSV file");
            Console.Out.WriteLine("           output.docm - Word DOCM file");
            Console.Out.WriteLine("           output.docx - Word DOCX file");
            Console.Out.WriteLine("           output.htm - HTML file with no CSS");
            Console.Out.WriteLine("           output.html - HTML file with CSS");
            Console.Out.WriteLine("           output.pdf - Acrobat PDF file");
            Console.Out.WriteLine("           output.pptm - PowerPoint PPTM file");
            Console.Out.WriteLine("           output.pptx - PowerPoint PPTX file");
            Console.Out.WriteLine("           output.rtf - Rich Text Format file");
            Console.Out.WriteLine("           output.sml - SpreadsheetML file (rename to .xml to use)");
            Console.Out.WriteLine("           output.txt - Ascii text file");
            Console.Out.WriteLine("           output.xhtml - XHTML file with CSS");
            Console.Out.WriteLine("           output.xls - Excel XLS file");
            Console.Out.WriteLine("           output.xlsm - Excel XLSM file");
            Console.Out.WriteLine("           output.xlsx - Excel XLSX file");
            Console.Out.WriteLine("           output.xml - WordML file");
            Console.Out.WriteLine("       -performance:123 - will run the report 123 times.");
            Console.Out.WriteLine("            output file is used for directory and extension for reports");
            Console.Out.WriteLine("       -threads:4 - will create 4 threads when running -performance.");
            Console.Out.WriteLine("       -launch - will launch the report when complete.");
            Console.Out.WriteLine("       -data filename.cml - will write data.xml to this filename.");
            Console.Out.WriteLine("       -embed - will embed data.xml in the generated report. DOCX, PDF, PPTX, & XLSX only.");
            Console.Out.WriteLine("       version=9 - sets the template to the passed version (9 in this example)");
            Console.Out.WriteLine("       -record filename - records the next datasource to this file");
            Console.Out.WriteLine("       The datasource is identified with a pair of parameters");
            Console.Out.WriteLine("           -xml filename - passes an xml file as the datasource");
            Console.Out.WriteLine("                -xml xmlFilename;schemaFilename - passes an xml file and a schema file as the datasource");
            foreach (AdoDriverInfo di in AdoDriverInfo.drivers)
                Console.Out.WriteLine("           -" + di.Name + " connection_string ex: " + di.Example);
            Console.Out.WriteLine("               if a datasource is named you use the syntax -type:name (ex: -xml:name filename.xml)");
            Console.Out.WriteLine("               set username=user password=pass BEFORE datasource for database connections");
            Console.Out.WriteLine("               for a POD file (datasets), set pod=pod_filename");
            Console.Out.WriteLine("                    must come BEFORE each -xml, -sql, ... part");
            Console.Out.WriteLine("           -rest filename - passes an xml file as the datasource reading it with the REST protocol");
            Console.Out.WriteLine("           -odata url - passes a url as the datasource accessing it using the OData protocol");
            Console.Out.WriteLine("           -sforce - password should be password + security_token");
            Console.Out.WriteLine("           -sharepoint filename - passes an xml file as the datasource reading it with the SharePoint FBA protocol");
            Console.Out.WriteLine("           -playback filename - passes an recorded file as the datasource");
            Console.Out.WriteLine("       You can have 0-N key=value pairs that are passed to the datasource Map property");
            Console.Out.WriteLine("            If the value starts with I', F', or D' it parses it as an integer, float, or date(yyyy-MM-ddThh:mm:ss)");
            Console.Out.WriteLine("            If the value is * it will set a filter of all");
            Console.Out.WriteLine("            If the value is \"text,text,...\" it will set a filter of all");
        }
    }

    /// <summary>
    /// This class contains everything passed in the command line. It makes no calls to Windward Reports.
    /// </summary>
    internal class CommandLine {
        /// <summary>
        /// Create the object.
        /// </summary>
        /// <param name="templateFilename">The name of the template file.</param>
        /// <param name="reportFilename">The name of the report file. null for printer reports.</param>
        public CommandLine(string templateFilename, string reportFilename) {
            TemplateFilename = FileUtils.FullPath(templateFilename);
            if (!reportFilename.ToLower().EndsWith(".prn"))
                reportFilename = Path.GetFullPath(reportFilename);
            Launch = false;
            ReportFilename = reportFilename;
            Map = new Dictionary<string, object>();
            Datasources = new List<DatasourceInfo>();
            NumThreads = Environment.ProcessorCount * 2;
        }

        public CommandLine(CommandLine src, string report) {
            TemplateFilename = src.TemplateFilename;
            Map = src.Map;
            Datasources = src.Datasources;
            Locale = src.Locale;
            Launch = src.Launch;
            TemplateVersion = src.TemplateVersion;
            NumReports = src.NumReports;
            NumThreads = src.NumThreads;
            DataMode = src.DataMode;
            DataFileName = src.DataFileName;
            BaseDirectory = src.BaseDirectory;

            ReportFilename = report;
        }

        /// <summary>
        /// The name of the template file.
        /// </summary>
        public string TemplateFilename { get; private set; }

        /// <summary>
        /// The name of the report file. null for printer reports.
        /// </summary>
        public string ReportFilename { get; private set; }

        /// <summary>
        /// true if launch the app at the end.
        /// </summary>
        public bool Launch { get; private set; }

        /// <summary>
        /// The template version number. 0 if not set.
        /// </summary>
        public int TemplateVersion { get; private set; }

        /// <summary>
        /// The name/value pairs for variables passed to the datasources.
        /// </summary>
        public Dictionary<string, object> Map { get; private set; }

        /// <summary>
        /// The parameters passed for each datasource to be created.
        /// </summary>
        public List<DatasourceInfo> Datasources { get; private set; }

        /// <summary>
        /// The locale to run under.
        /// </summary>
        public string Locale { get; private set; }

        /// <summary>
        /// For performance modeling, how many reports to run.
        /// </summary>
        public int NumReports { get; private set; }

        /// <summary>
        /// true if requesting a performance run
        /// </summary>
        public bool IsPerformance {
            get { return NumReports != 0; }
        }

        /// <summary>
        /// The number of threads to launch if running a performance test.
        /// </summary>
        public int NumThreads { get; private set; }

        /// <summary>
        /// The data mode (data.xml) when running the report.
        /// </summary>
        public Report.DATA_MODE DataMode { get; private set; }

        /// <summary>
        /// The data.xml filename if written to disk.
        /// </summary>
        public string DataFileName { get; private set; }

        /// <summary>
        /// Base directory.
        /// </summary>
        public string BaseDirectory { get; private set; }

        /// <summary>
        /// The parameters passed for a single datasource. All filenames are expanded to full paths so that if an exception is
        /// thrown you know exactly where the file is.
        /// </summary>
        internal class DatasourceInfo {
            /// <summary>
            /// What type of datasource.
            /// </summary>
            internal enum TYPE {
                /// <summary>
                /// Use the REST protocol passing the credentials on the first request.
                /// </summary>
                REST,
                /// <summary>
                /// Use the SharePoint protocol for FBA.
                /// </summary>
                SHAREPOINT,
                /// <summary>
                /// A SQL database.
                /// </summary>
                SQL,
                /// <summary>
                /// An XML file.
                /// </summary>
                XML,
                /// <summary>
                /// An OData url.
                /// </summary>
                ODATA,
                /// <summary>
                /// JSON data source.
                /// </summary>
                JSON,
                /// <summary>
                /// SalesForce data source
                /// </summary>
                SFORCE
            }

            private readonly TYPE type;
            private readonly string name;

            private readonly string filename;
            private readonly string schemaFilename;

            private readonly AdoDriverInfo sqlDriverInfo;
            private readonly string connectionString;

            private readonly string username;
            private readonly string password;
            private readonly string securitytoken;//only used for sforce
            private readonly string podFilename;

            /// <summary>
            /// Create the object for a PLAYBACK datasource.
            /// </summary>
            /// <param name="filename">The playback filename.</param>
            /// <param name="type">What type of datasource.</param>
            public DatasourceInfo(string filename, TYPE type) {
                this.filename = Path.GetFullPath(filename);
                this.type = type;
            }

            /// <summary>
            /// Create the object for a XML datasource.
            /// </summary>
            /// <param name="name">The name for this datasource.</param>
            /// <param name="filename">The XML filename.</param>
            /// <param name="schemaFilename">The XML schema filename. null if no schema.</param>
            /// <param name="username">The username if credentials are needed to access the datasource.</param>
            /// <param name="password">The password if credentials are needed to access the datasource.</param>
            /// <param name="podFilename">The POD filename if datasets are being passed.</param>
            /// <param name="type">What type of datasource.</param>
            public DatasourceInfo(string name, string filename, string schemaFilename, string username, string password, string podFilename, TYPE type) {
                this.name = name ?? string.Empty;
                this.filename = GetFullPath(filename);
                if (!string.IsNullOrEmpty(schemaFilename))
                    this.schemaFilename = GetFullPath(schemaFilename);
                this.username = username;
                this.password = password;
                if (!string.IsNullOrEmpty(podFilename))
                    this.podFilename = GetFullPath(podFilename);
                this.type = type;
            }

            private static string GetFullPath(string filename) {
                int pos = filename.IndexOf(':');
                if ((pos == -1) || (pos == 1))
                    return Path.GetFullPath(filename);
                return filename;
            }

            /// <summary>
            /// Create the object for a SQL datasource.
            /// </summary>
            /// <param name="name">The name for this datasource.</param>
            /// <param name="sqlDriverInfo">The DriverInfo for the selected SQL vendor.</param>
            /// <param name="connectionString">The connection string to connect to the database.</param>
            /// <param name="username">The username if credentials are needed to access the datasource.</param>
            /// <param name="password">The password if credentials are needed to access the datasource.</param>
            /// <param name="podFilename">The POD filename if datasets are being passed.</param>
            /// <param name="type">What type of datasource.</param>
            public DatasourceInfo(string name, AdoDriverInfo sqlDriverInfo, string connectionString, string username, string password, string podFilename, TYPE type) {
                this.name = name;
                this.sqlDriverInfo = sqlDriverInfo;
                this.connectionString = connectionString.Trim();
                this.username = username;
                this.password = password;
                if (!string.IsNullOrEmpty(podFilename))
                    this.podFilename = Path.GetFullPath(podFilename);
                this.type = type;
            }

            /// <summary>
            /// What type of datasource.
            /// </summary>
            public TYPE Type {
                get { return type; }
            }

            /// <summary>
            /// The name for this datasource.
            /// </summary>
            public string Name {
                get { return name; }
            }

            /// <summary>
            /// The XML or playback filename.
            /// </summary>
            public string Filename {
                get { return filename; }
            }

            /// <summary>
            /// The XML schema filename. null if no schema.
            /// </summary>
            public string SchemaFilename {
                get { return schemaFilename; }
            }

            /// <summary>
            /// The DriverInfo for the selected SQL vendor.
            /// </summary>
            public AdoDriverInfo SqlDriverInfo {
                get { return sqlDriverInfo; }
            }

            /// <summary>
            /// The connection string to connect to the database.
            /// </summary>
            public string ConnectionString {
                get { return connectionString; }
            }

            /// <summary>
            /// The username if credentials are needed to access the datasource.
            /// </summary>
            public string Username {
                get { return username; }
            }

            /// <summary>
            /// The password if credentials are needed to access the datasource.
            /// </summary>
            public string Password {
                get { return password; }
            }

            /// <summary>
            /// The POD filename if datasets are being passed.
            /// </summary>
            public string PodFilename {
                get { return podFilename; }
            }
        }

        /// <summary>
        /// Create a CommandLine object from the command line passed to the program.
        /// </summary>
        /// <param name="args">The arguments passed to the program.</param>
        /// <returns>A CommandLine object populated from the args.</returns>
        public static CommandLine Factory(IList<string> args) {

            CommandLine rtn = new CommandLine(args[0], args[1]);

            string username = null, password = null, podFilename = null;

            for (int ind = 2; ind < args.Count; ind++) {
                string[] sa = args[ind].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                string cmd = sa[0].Trim();
                string name = sa.Length < 2 ? "" : sa[1].Trim();

                if (cmd == "-embed") {
                    rtn.DataMode = Report.DATA_MODE.ALL_ATTRIBUTES | Report.DATA_MODE.DATA | Report.DATA_MODE.EMBED;
                    continue;
                }

                if (cmd == "-data") {
                    rtn.DataMode = Report.DATA_MODE.ALL_ATTRIBUTES | Report.DATA_MODE.DATA;
                    rtn.DataFileName = Path.GetFullPath(args[++ind]);
                    continue;
                }

                if (cmd == "-performance") {
                    rtn.NumReports = int.Parse(name);
                    continue;
                }

                if (cmd == "-threads") {
                    rtn.NumThreads = int.Parse(name);
                    continue;
                }

                if (cmd == "-launch") {
                    rtn.Launch = true;
                    continue;
                }

                if (cmd == "-basedir") {
                    rtn.BaseDirectory = args[++ind];
                    continue;
                }

                if ((cmd == "-xml") || (cmd == "-rest") || (cmd == "-sharepoint")) {
                    string filename = args[++ind];
                    string schemaFilename = null;
                    string[] parts = filename.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1) {
                        filename = parts[0].Trim();
                        schemaFilename = parts[1].Trim();
                    }
                    DatasourceInfo.TYPE type = cmd == "-sharepoint" ? DatasourceInfo.TYPE.SHAREPOINT : (cmd == "-rest" ? DatasourceInfo.TYPE.REST : DatasourceInfo.TYPE.XML);
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, filename, schemaFilename, username, password, podFilename, type);
                    rtn.Datasources.Add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd == "-odata") {
                    string url = args[++ind];
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, null, username, password, podFilename, DatasourceInfo.TYPE.ODATA);
                    rtn.Datasources.Add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd == "-json") {
                    string url = args[++ind];
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, null, username, password, podFilename, DatasourceInfo.TYPE.JSON);
                    rtn.Datasources.Add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }


                bool isDb = false;

                if (cmd == "-sforce") {
                    string url = "https://login.salesforce.com";
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, null, username, password, podFilename, DatasourceInfo.TYPE.SFORCE);
                    rtn.Datasources.Add(datasourceOn);
                    isDb = true;
                    username = password = podFilename = null;
                }
                foreach (AdoDriverInfo di in AdoDriverInfo.drivers)
                    if (cmd == "-" + di.Name) {
                        if (((di.Name == "odbc") || (di.Name == "oledb")) && (IntPtr.Size != 4))
                            Console.Out.WriteLine("Warning - some ODBC & OleDB connectors only work in 32-bit mode.");

                        DatasourceInfo datasourceOn = new DatasourceInfo(name, di, args[++ind], username, password, podFilename, DatasourceInfo.TYPE.SQL);
                        rtn.Datasources.Add(datasourceOn);
                        isDb = true;
                        username = password = podFilename = null;
                        break;
                    }
                if (isDb)
                    continue;

                // assume this is a key=value
                string[] keyValue = args[ind].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length != 2)
                    if (keyValue.Length == 1 && args[ind].Contains("=")) {
                        // We have a variable with the empty value.
                        keyValue = new string[2] { keyValue[0], "" };
                    }
                    else
                        throw new ArgumentException(string.Format("Unknown option {0}", args[ind]));

                switch (keyValue[0]) {
                    case "locale":
                        rtn.Locale = keyValue[1];
                        break;
                    case "version":
                        rtn.TemplateVersion = int.Parse(keyValue[1]);
                        break;
                    case "username":
                        username = keyValue[1];
                        break;
                    case "password":
                        password = keyValue[1];
                        break;
                    case "pod":
                        podFilename = keyValue[1];
                        break;
                    default:
                        object value;
                        // may be a list
                        if (keyValue[1].IndexOf(',') != -1) {
                            string[] tok = keyValue[1].Split(new char[] { ',' });
                            IList items = new ArrayList();
                            foreach (string elem in tok)
                                items.Add(ConvertValue(elem));
                            value = items;
                        }
                        else if (keyValue[1] == "*")
                            value = new FilterCondition(FilterBase.SORT_ORDER.NO_OVERRIDE);
                        else
                            value = ConvertValue(keyValue[1]);
                        rtn.Map.Add(keyValue[0], value);
                        break;
                }
            }

            return rtn;
        }

        private static object ConvertValue(string keyValue) {
            if (keyValue.StartsWith("I'"))
                return Convert.ToInt64(keyValue.Substring(2));
            if (keyValue.StartsWith("F'"))
                return Convert.ToDouble(keyValue.Substring(2));
            if (keyValue.StartsWith("D'"))
                return Convert.ToDateTime(keyValue.Substring(2));
            return keyValue;
        }
    }

    /// <summary>
    /// Information on all known ADO.NET connectors.
    /// </summary>
    internal class AdoDriverInfo {
        private readonly String name;
        private readonly String classname;
        private readonly String example;

        /// <summary>
        /// Create the object for a given vendor.
        /// </summary>
        /// <param name="name">The -vendor part in the command line (ex: -sql).</param>
        /// <param name="classname">The classname of the connector.</param>
        /// <param name="example">A sample commandline.</param>
        public AdoDriverInfo(string name, string classname, string example) {
            this.name = name;
            this.classname = classname;
            this.example = example;
        }

        /// <summary>
        /// The -vendor part in the command line (ex: -sql).
        /// </summary>
        public string Name {
            get { return name; }
        }

        /// <summary>
        /// The classname of the connector.
        /// </summary>
        public string Classname {
            get { return classname; }
        }

        /// <summary>
        /// A sample commandline.
        /// </summary>
        public string Example {
            get { return example; }
        }

        internal static readonly AdoDriverInfo[] drivers = {
                            new AdoDriverInfo("db2", "IBM.Data.DB2", "server=localhost;database=SAMPLE;Uid=test;Pwd=pass;"),
                            new AdoDriverInfo("excel", "System.Data.OleDb", "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=c:\\test1.xlsx;Extended Properties=\"Excel 12.0 Xml;HDR=YES\""),
                            new AdoDriverInfo("mysql", "MySql.Data.MySqlClient", "server=localhost;database=sakila;user id=test;password=pass;"),
                            new AdoDriverInfo("odbc", "System.Data.Odbc", "Driver={Sql Server};Server=localhost;Database=Northwind;User ID=test;Password=pass;"),
                            new AdoDriverInfo("oledb", "System.Data.OleDb", "Provider=sqloledb;Data Source=localhost;Initial Catalog=Northwind;User ID=test;Password=pass;"),
                            new AdoDriverInfo("oracle", "Oracle.DataAccess.Client", "Data Source=localhost:1521/HR;Persist Security Info=True;Password=HR;User ID=HR"),
                            new AdoDriverInfo("sql", "System.Data.SqlClient", "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=SSPI;"),
                            new AdoDriverInfo("postgresql", "Npgsql", "HOST=localhost;DATABASE=pagila;USER ID=test;PASSWORD=test;"),
                            };
    }
}