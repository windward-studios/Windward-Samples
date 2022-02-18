using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ikvm.extensions;
using Kailua.net.windward.utils;
using Kailua.net.windward.utils.ado;
using log4net;
using log4net.Config;
using net.windward.api.csharp;
using net.windward.api.csharp.errorhandling;
using net.windward.env;
using net.windward.util.AccessProviders;
using net.windward.utils.ado;
using net.windward.utils.ado.Redshift;
using WindwardInterfaces.net.windward.api.csharp;
using WindwardInterfaces.net.windward.datasource;

namespace RunReport
{
    /// <summary>
    /// A sample usage of Windward Reports. This program generates reports based on the command line.
    /// This project is used for two purposes, as a sample and as a way to run reports easily from the command line (mostly for testing).
    /// The second ues provides one downside and one upside as a sample. The downside is it includes items like the recorder that would
    /// not be included if this was solely as a sample. The upside is it does pretty much everything because of the needs as a way to
    /// test any report.
    /// 
    /// To get the parameters, the RunReport with no parameters and it will list them all out.
    /// </summary>
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static bool askForKeystrokeEachStep = false;

        /// <summary>
        /// Create a report using Windward Reports.
        /// </summary>
        /// <param name="args">run with no parameters to list out usage.</param>
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            log.Info($"Starting RunReport ({string.Join(", ", args)})");

            // if no arguments, then we list out the usage.
            if (args.Length < 2)
            {
                DisplayUsage();
                return;
            }

            // the try here is so we can print out an exception if it is thrown. This code does minimal error checking and no other
            // exception handling to keep it simple & clear.
            try
            {
                // Initialize the reporting engine. This will throw an exception if the engine is not fully installed or you
                // do not have a valid license key in RunReport.exe.config.
                Report.Init();
                Console.Out.WriteLine("Running in {0}-bit mode", IntPtr.Size * 8);

                // parse the arguments passed in. This method makes no calls to Windward, it merely organizes the passed in arguments.
                CommandLine cmdLine = CommandLine.Factory(args);

                DateTime startTime = DateTime.Now;

                // run one report
                if (!cmdLine.IsPerformance)
                {
                    PerfCounters perfCounters = RunOneReport(cmdLine, args.Length == 2);
                    PrintPerformanceCounter(startTime, perfCounters, false);
                }
                else
                {
                    string dirReports = Path.GetDirectoryName(Path.GetFullPath(cmdLine.ReportFilename)) ?? "";
                    if (!Directory.Exists(dirReports))
                    {
                        Console.Error.WriteLine($"The directory {dirReports} does not exist");
                        return;
                    }

                    // load the template once (otherwise once per thread)
                    if (cmdLine.Cache)
                    {
                        Stream stream = cmdLine.GetTemplateStream();
                        stream.Close();
                    }

                    // drop out threads - twice the number of cores.
                    int numThreads = cmdLine.NumThreads;
                    numReportsRemaining = cmdLine.NumReports;
                    ReportWorker[] workers = new ReportWorker[numThreads];
                    for (int ind = 0; ind < numThreads; ind++)
                        workers[ind] = new ReportWorker(ind, new CommandLine(cmdLine));
                    System.Threading.Thread[] threads = new System.Threading.Thread[numThreads];
                    for (int ind = 0; ind < numThreads; ind++)
                        threads[ind] = new System.Threading.Thread(workers[ind].DoWork);
                    for (int ind = 0; ind < numThreads; ind++)
                        threads[ind].Name = "Report Worker " + ind;

                    Console.Out.WriteLine($"Start time: {startTime.ToLongTimeString()}, {numThreads} threads, {cmdLine.NumReports} reports");
                    Console.Out.WriteLine();
                    for (int ind = 0; ind < numThreads; ind++)
                        threads[ind].Start();

                    // we wait
                    for (int ind = 0; ind < numThreads; ind++)
                        threads[ind].Join();

                    PerfCounters perfCounters = new PerfCounters();
                    for (int ind = 0; ind < numThreads; ind++)
                        perfCounters.Add(workers[ind].perfCounters);

                    Console.Out.WriteLine();
                    PrintPerformanceCounter(startTime, perfCounters, true);
                }

                // need to call this here after all threads have completed (not in the per thread code)
                if (DbConnectionWrapper.HasOpenConnections)
                {
                    Console.Out.WriteLine("Warning: Did not close all database connections. Open connections written to log.");
                    DbConnectionWrapper.LogOpenConnections();
                }

                Report.Shutdown();
            }
            catch (Exception ex)
            {
                log.Error("RunReport", ex);
                if (DbConnectionWrapper.HasOpenConnections)
                    DbConnectionWrapper.LogOpenConnections();
                string indent = "  ";
                Console.Error.WriteLine();
                Console.Error.WriteLine("Error(s) running the report");
                while (ex != null)
                {
                    Console.Error.WriteLine($"{indent}Error: {ex.Message} ({ex.GetType().FullName})\n{indent}     stack: {ex.StackTrace}\n");
                    ex = ex.InnerException;
                    indent += "  ";
                }
                throw;
            }
        }

        private static void PrintPerformanceCounter(DateTime startTime, PerfCounters perfCounters, bool multiThreaded)
        {

            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime - startTime;

            Console.Out.WriteLine("End time: {0}", endTime.ToLongTimeString());
            Console.Out.WriteLine($"Elapsed time: {elapsed}");
            Console.Out.WriteLine("Time per report: {0}", perfCounters.numReports == 0 ? "n/a" : "" + TimeSpan.FromMilliseconds(elapsed.TotalMilliseconds / perfCounters.numReports));
            Console.Out.WriteLine("Pages/report: " + (perfCounters.numReports == 0 ? "n/a" : "" + perfCounters.numPages / perfCounters.numReports));
            Console.Out.WriteLine("Pages/sec: {0:N2}", perfCounters.numPages / elapsed.TotalSeconds);
            if (multiThreaded)
                Console.Out.WriteLine("Below values are totaled across all threads (and so add up to more than the elapsed time)");
            Console.Out.WriteLine($"  Setup & parse: {perfCounters.timeSetup}");
            Console.Out.WriteLine($"  Data: {perfCounters.timeApplyData}");
            Console.Out.WriteLine($"  Layout: {perfCounters.timeLayout}");
            Console.Out.WriteLine($"  Build output: {perfCounters.timeOutput}");
        }


        private static int numReportsRemaining;

        private static bool HasNextReport
        {
            get
            {
                return Interlocked.Decrement(ref numReportsRemaining) >= 0;
            }
        }

        private class ReportWorker
        {
            private readonly int threadNum;
            private readonly CommandLine cmdLine;
            internal PerfCounters perfCounters;

            public ReportWorker(int threadNum, CommandLine cmdLine)
            {
                this.threadNum = threadNum;
                this.cmdLine = cmdLine;
                perfCounters = new PerfCounters();
            }

            public void DoWork()
            {
                while (HasNextReport)
                {
                    Console.Out.Write($"{threadNum}.");
                    PerfCounters pc = RunOneReport(cmdLine, false);
                    perfCounters.Add(pc);
                }
            }
        }

        private static string[] extImages = { "bmp", "eps", "gif", "jpg", "png", "svg" };
        private static string[] extHtmls = { "htm", "html", "xhtml" };

        private static PerfCounters RunOneReport(CommandLine cmdLine, bool preservePodFraming)
        {

            DateTime startTime = DateTime.Now;
            PerfCounters perfCounters = new PerfCounters();

            // get the template and output file streams. Output is null for printers
            using (Stream template = cmdLine.GetTemplateStream())
            {
                Stream output;
                if ((extHtmls.Any(fn => cmdLine.ReportFilename.ToLower().EndsWith(fn))
                    && ReportHtml.SplitPagesStatic) || extImages.Any(fn => cmdLine.ReportFilename.ToLower().EndsWith(fn)))
                    output = null;
                else if (!cmdLine.ReportFilename.ToLower().EndsWith(".prn"))
                    output = cmdLine.GetOutputStream();
                else
                    output = null;
                if (!cmdLine.IsPerformance)
                {
                    Console.Out.WriteLine(string.Format("Template: {0}", cmdLine.TemplateFilename));
                    Console.Out.WriteLine(string.Format("Report: {0}", cmdLine.ReportFilename));
                }

                // Create the report object, based on the file extension
                using (Report report = CreateReport(cmdLine, template, output))
                {
                    // if (!cmdLine.IsPerformance)
                    // {
                    //     Console.Out.WriteLine(string.Format("Data processor: {0}", report.DataProcessorVersion));
                    // }

                    /*report.OutputBuilder = new PDFTronOutputBuilder();
					report.UseExternalOutputBuilder = true;*/

                    /*report.Properties.Set("output.builder", "net.windward.env.PDFTronOutputBuilder");
					report.Properties.Set("use.external.output.builder", true);*/

                    report.TrackErrors = (Report.ERROR_HANDLING)cmdLine.VerifyFlag;

                    if (cmdLine.BaseDirectory != null)
                        report.BaseDirectory = cmdLine.BaseDirectory;
                    else
                        report.BaseDirectory = Path.GetDirectoryName(cmdLine.TemplateFilename);

                    // if we are applying no datasources then we keep the POD framing in the generated report.
                    if (preservePodFraming)
                        report.PreservePodFraming = true;
                    // if we have a locale, we set it (used when applying datasources).
                    if (cmdLine.Locale != null)
                    {
                        report.Locale = cmdLine.Locale;
                        if (!cmdLine.IsPerformance)
                        {
                            CultureInfo ci = new CultureInfo(report.Locale.Replace("_", "-"));
                            Console.Out.WriteLine($"Using locale: {ci} ({ci.DisplayName})");
                        }
                    }
                    if (cmdLine.TemplateVersion != 0)
                        report.TemplateVersion = cmdLine.TemplateVersion;

                    // the data.xml file
                    Stream dataStream = null;
                    if (cmdLine.DataMode != 0)
                    {
                        report.DataMode = cmdLine.DataMode;
                        if (cmdLine.DataFileName != null)
                        {
                            dataStream = new FileStream(cmdLine.DataFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                            report.DataStream = dataStream;
                        }
                    }

                    // This first call parses the template and prepares the report so we can apply data to it.
                    report.ProcessSetup();

                    DateTime postSetup = DateTime.Now;
                    perfCounters.timeSetup = postSetup - startTime;

                    IDictionary<string, IReportDataSource> dataProviders = null;
                    if (cmdLine.Cache)
                        dataProviders = cmdLine.DataProviders;
                    if (dataProviders == null)
                        dataProviders = new Dictionary<string, IReportDataSource>();

                    if (askForKeystrokeEachStep)
                    {
                        Console.Out.WriteLine("setup done - press any key to continue.");
                        Console.ReadKey();
                        Console.Out.WriteLine("continuing...");
                    }

                    // Now for each datasource, we apply it to the report. This is complex because it handles all datasource types
                    // as well as recording and playback.
                    if (dataProviders.Count == 0)
                        foreach (CommandLine.DatasourceInfo dsInfo in cmdLine.Datasources)
                        {
                            // build the datasource
                            IReportDataSource datasource;
                            Stream dsStream = null;
                            Stream schemaStream = null;
                            switch (dsInfo.Type)
                            {
                                // An XPath datasource.
                                case CommandLine.DatasourceInfo.TYPE.XML:
                                    if (!cmdLine.IsPerformance)
                                    {
                                        PrintXPathInfo(dsInfo);
                                    }
                                    datasource = new SaxonDataSourceImpl(dsInfo.ExConnectionString, dsInfo.SchemaFilename);
                                    break;

                                // An XPATH 1.0 datasource.
                                case CommandLine.DatasourceInfo.TYPE.XPATH_1:
                                    if (!cmdLine.IsPerformance)
                                    {
                                        PrintXPathInfo(dsInfo);
                                    }
                                    datasource = new XmlDataSourceImpl(dsInfo.ExConnectionString, dsInfo.SchemaFilename);
                                    break;

                                case CommandLine.DatasourceInfo.TYPE.JSON:
                                    if (!cmdLine.IsPerformance)
                                        Console.Out.WriteLine($"JSON datasource: {dsInfo.Filename}");
                                    datasource = new JsonDataSourceImpl(dsInfo.Filename, JsonDataSourceImpl.MODE.CONNECTION_STRING);
                                    break;


                                // An OData datasource.
                                case CommandLine.DatasourceInfo.TYPE.ODATA:
                                    if (!cmdLine.IsPerformance)
                                        Console.Out.WriteLine(string.Format("OData datasource: {0}", dsInfo.Filename));

                                    datasource = new ODataDataSourceImpl(dsInfo.ExConnectionString);
                                    // assign the loaded datasets. 
                                    /* bugbug
									if (datasets != null)
										((ODataDataSourceImpl)datasource).Datasets = (ODataDataset[])datasets;
									*/
                                    break;

                                // A SalesForce datasource.
                                case CommandLine.DatasourceInfo.TYPE.SFORCE:
                                    if (!cmdLine.IsPerformance)
                                        Console.Out.WriteLine(string.Format("SalesForce datasource: {0}", dsInfo.Filename));
                                    datasource = new SFDataSourceImpl(dsInfo.Username, dsInfo.Password, true);
                                    break;

                                case CommandLine.DatasourceInfo.TYPE.DATA_SET:
                                    if (!cmdLine.IsPerformance)
                                        Console.Out.WriteLine("DATASET datasource: " + dsInfo.Filename);

                                    string dataSetStr = dsInfo.Filename;
                                    string[] dataSetArgs = dataSetStr.split(";");
                                    if (dataSetArgs.Length != 2)
                                        throw new ArgumentException("A the dataset string must be in the format \"ds=dataSourceName;select=Select\"");
                                    string ds = null;
                                    string select = null;
                                    for (int i = 0; i < dataSetArgs.Length; i++)
                                    {
                                        string[] argOn = dataSetArgs[i].split("=");
                                        if (argOn.Length != 2)
                                            throw new ArgumentException("A the dataset string must be in the format \"ds=dataSourceName;select=Select\"");
                                        if (argOn[0].Equals("ds"))
                                        {
                                            ds = argOn[1];
                                            continue;
                                        }
                                        if (argOn[0].Equals("select"))
                                        {
                                            select = argOn[1];
                                            continue;
                                        }
                                        throw new ArgumentException("A the dataset string must be in the format \"ds=dataSourceName;select=Select\"");
                                    }
                                    datasource = new DataSetImpl(dsInfo.Name, select, dataProviders[ds]);
                                    break;

                                case CommandLine.DatasourceInfo.TYPE.SQL:
                                    if (!cmdLine.IsPerformance)
                                        Console.Out.WriteLine(string.Format("{0} datasource: {1}", dsInfo.SqlDriverInfo.Name,
                                            dsInfo.ConnectionString));
                                    datasource = new AdoDataSourceImpl(dsInfo.SqlDriverInfo.Classname, dsInfo.ConnectionString);
                                    break;

                                default:
                                    throw new ArgumentException(string.Format("Unknown datasource type {0}", dsInfo.Type));
                            }
                            if (!cmdLine.IsPerformance)
                            {
                                if (!string.IsNullOrEmpty(dsInfo.Username))
                                    Console.Out.WriteLine("    username={0}", dsInfo.Username);
                                if (!string.IsNullOrEmpty(dsInfo.Password))
                                    Console.Out.WriteLine("    password={0}", dsInfo.Password);
                                if (dsInfo.PodFilename != null)
                                    Console.Out.WriteLine("    POD filename={0}", dsInfo.PodFilename);
                            }

                            // We give all datasources all map variables. You can give each just some if needed.

                            dataProviders.Add(dsInfo.Name, datasource);

                            // because of the switch above we have to explicitly close instead of structuring as a using{}
                            if (dsStream != null)
                                dsStream.Close();
                            if (schemaStream != null)
                                schemaStream.Close();
                        }

                    // this applies the datasource to the report populating the tags.
                    if (dataProviders.Count > 0)
                    {
                        report.Parameters = cmdLine.Parameters;
                        report.ProcessData(dataProviders);
                    }

                    if (cmdLine.Cache)
                        cmdLine.DataProviders = dataProviders;
                    else
                        foreach (IReportDataSource provider in dataProviders.Values)
                            provider.Close();

                    if (!cmdLine.IsPerformance)
                        Console.Out.WriteLine("all data applied, generating report");

                    if (askForKeystrokeEachStep)
                    {
                        Console.Out.WriteLine("data done - press any key to continue.");
                        Console.ReadKey();
                        Console.Out.WriteLine("continuing...");
                    }

                    DateTime postData = DateTime.Now;
                    perfCounters.timeApplyData = postData - postSetup;

                    // Now that all the data has been applied, we generate the final output report. This does the
                    // page layout and then writes out the output file.
                    long layoutTime = report.ProcessComplete();

                    perfCounters.timeLayout = TimeSpan.FromMilliseconds(layoutTime);
                    perfCounters.timeOutput = DateTime.Now - postData - perfCounters.timeLayout;
                    perfCounters.numPages = report.NumPages;
                    perfCounters.numReports = 1;

                    if (askForKeystrokeEachStep)
                    {
                        Console.Out.WriteLine("layout done - press any key to continue.");
                        Console.ReadKey();
                        Console.Out.WriteLine("continuing...");
                    }

                    PrintVerify(report);

                    // If it is an html report and has images, and embedded images option not chosen, we write these out
                    if (report is ReportHtml && !((ReportHtml)report).EmbedImages)
                    {
                        Trap.trap(((ReportHtml)report).Bitmaps.Length > 0);
                        string dir = Path.GetDirectoryName(Path.GetFullPath(cmdLine.ReportFilename));
                        foreach (HtmlBitmap bitmap in ((ReportHtml)report).Bitmaps)
                        {
                            string filename = Path.Combine(dir, bitmap.Filename);
                            if (!cmdLine.IsPerformance)
                                Console.Out.WriteLine(string.Format("Saving image {0}", filename));
                            using (FileStream bmFile = new FileStream(filename, FileMode.Create, FileAccess.Write))
                            {
                                bmFile.Write(bitmap.Filedata, 0, bitmap.Filedata.Length);
                            }
                        }
                    }

                    // close out data.xml
                    if (dataStream != null)
                    {
                        dataStream.Close();
                        Console.Out.WriteLine("data written to: " + cmdLine.DataFileName);
                    }

                    // Close the output template to get everything written. (No "using (...)" because for printer output it is null.)
                    if (output != null)
                        output.Close();

                    // Close output streams for HTML -- don't worry if split_pages en/disabled
                    ReportHtml htmlReport = report as ReportHtml;
                    if (htmlReport?.Pages != null)
                    {
                        List<byte[]> pages = htmlReport.Pages;
                        string prefix = Path.GetFileNameWithoutExtension(cmdLine.ReportFilename);
                        string directory = Path.GetDirectoryName(Path.GetFullPath(cmdLine.ReportFilename));
                        string extension = Path.GetExtension(cmdLine.ReportFilename);
                        for (int i = 0; i < pages.Count; i++)
                        {
                            string filename = Path.Combine(directory, prefix + "_" + i + extension);
                            filename = Path.GetFullPath(filename);
                            File.WriteAllBytes(filename, pages[i]);
                            Console.Out.WriteLine("HTML page written to " + filename);
                        }
                    }

                    // pages for images
                    ReportImage imageReport = report as ReportImage;
                    if (imageReport != null)
                    {
                        {
                            string prefix = Path.GetFileNameWithoutExtension(cmdLine.ReportFilename);
                            string directory = Path.GetDirectoryName(Path.GetFullPath(cmdLine.ReportFilename));
                            string extension = Path.GetExtension(cmdLine.ReportFilename);
                            for (int fileNumber = 0; fileNumber < imageReport.Pages.Count; fileNumber++)
                            {
                                string filename = Path.Combine(directory, prefix + "_" + fileNumber + extension);
                                filename = Path.GetFullPath(filename);
                                File.WriteAllBytes(filename, imageReport.Pages[fileNumber]);
                                Console.Out.WriteLine("HTML page written to " + filename);
                            }
                        }
                    }

                    if (!cmdLine.IsPerformance)
                    {
                        Console.Out.WriteLine($"{cmdLine.ReportFilename} built, {report.NumPages} pages long.");
                        Console.Out.WriteLine($"Elapsed time: {DateTime.Now - startTime}");
                    }

                    if (askForKeystrokeEachStep)
                    {
                        Console.Out.WriteLine("press any key to end.");
                        Console.ReadKey();
                    }

                    if (cmdLine.Launch)
                    {
                        Console.Out.WriteLine(string.Format("launching report {0}", cmdLine.ReportFilename));
                        System.Diagnostics.Process.Start(cmdLine.ReportFilename);
                    }
                }
            }
            return perfCounters;
        }

        private static void PrintXPathInfo(CommandLine.DatasourceInfo dsInfo)
        {
            if (string.IsNullOrEmpty(dsInfo.SchemaFilename))
            {
                Console.Out.WriteLine($"XPath {dsInfo.XPathVersion} datasource: {dsInfo.Filename}");
            }
            else
            {
                Console.Out.WriteLine($"XPath {dsInfo.XPathVersion} datasource {dsInfo.Filename}, schema {dsInfo.SchemaFilename}");
            }
        }

        private static void PrintVerify(Report report)
        {
            foreach (Issue issue in report.GetErrorInfo().Errors)
            {
                Console.Error.WriteLine(issue.Message);
            }
        }

        private static Report CreateReport(CommandLine cmdLine, Stream template, Stream output)
        {
            Report report;
            string ext = cmdLine.ReportFilename.Substring(cmdLine.ReportFilename.LastIndexOf('.') + 1).ToLower();
            switch (ext)
            {
                case "bmp":
                    return new ReportImage(template, ReportImage.FORMAT.BMP, 600);
                case "csv":
                    return new ReportCsv(template, output);
                case "docx":
                case "docm":
                    return new ReportDocx(template, output);
                case "eps":
                    return new ReportImage(template, ReportImage.FORMAT.EPS);
                case "gif":
                    return new ReportImage(template, ReportImage.FORMAT.GIF, 600);
                case "htm":
                    report = new ReportHtml(template, output);
                    ((ReportHtml)report).CssType = ReportHtml.CSS.NO;
                    return report;
                case "html":
                    report = new ReportHtml(template, output);
                    ((ReportHtml)report).CssType = ReportHtml.CSS.INCLUDE;
                    return report;
                case "jpg":
                case "jpeg":
                    return new ReportImage(template, ReportImage.FORMAT.JPG, 600);
                case "pdf":
                    return new ReportPdf(template, output);
                case "png":
                    return new ReportImage(template, ReportImage.FORMAT.PNG, 600);
                case "ps":
                    return new ReportPostScript(template, output);
                case "pptx":
                case "pptm":
                    return new ReportPptx(template, output);
                case "rtf":
                    return new ReportRtf(template, output);
                case "svg":
                    return new ReportImage(template, ReportImage.FORMAT.SVG);
                case "tif":
                case "tiff":
                    return new ReportImage(template, ReportImage.FORMAT.TIF, 600);
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

        private static void DisplayUsage()
        {
            Console.Out.WriteLine("Windward Reports version " + Report.Version);
            Console.Out.WriteLine("usage: RunReport template_file output_file [-basedir path] [-xml xml_file | -sql connection_string | -sforce | -oracle connection_string | -ole oledb_connection_string] [key=value | ...]");
            Console.Out.WriteLine("       The template file can be a docx, pptx, or xlsx file.");
            Console.Out.WriteLine("       The output file extension determines the report type created:");
            Console.Out.WriteLine("           output.csv - SpreadSheet CSV file");
            Console.Out.WriteLine("           output.docm - Word DOCM file");
            Console.Out.WriteLine("           output.docx - Word DOCX file");
            Console.Out.WriteLine("           output.htm - HTML file with no CSS");
            Console.Out.WriteLine("           output.html - HTML file with CSS");
            Console.Out.WriteLine("           output.pdf - Acrobat PDF file");
            Console.Out.WriteLine("           output.pptm - PowerPoint PPTM file");
            Console.Out.WriteLine("           output.pptx - PowerPoint PPTX file");
            Console.Out.WriteLine("           output.prn - Printer where \"output\" is the printer name");
            Console.Out.WriteLine("           output.rtf - Rich Text Format file");
            Console.Out.WriteLine("           output.txt - Ascii text file");
            Console.Out.WriteLine("           output.xhtml - XHTML file with CSS");
            Console.Out.WriteLine("           output.xlsm - Excel XLSM file");
            Console.Out.WriteLine("           output.xlsx - Excel XLSX file");
            Console.Out.WriteLine("       -basedir - will set the base directory to this.");
            Console.Out.WriteLine("       -data filename.xml - will write data.xml to this filename.");
            Console.Out.WriteLine("       -embed - will embed data.xml in the generated report. DOCX, PDF, PPTX, & XLSX only.");
            Console.Out.WriteLine("       -launch - will launch the report when complete.");
            Console.Out.WriteLine("       -performance:123 - will run the report 123 times.");
            Console.Out.WriteLine("            output file is used for directory and extension for reports");
            Console.Out.WriteLine("       -cache - will cache template & datasources, will write output to memory stream. Only used with -performance.");
            Console.Out.WriteLine("       -threads:4 - will create 4 threads when running -performance.");
            Console.Out.WriteLine("       -verify:N - turn on the error handling and verify feature where N is a number: 0 (none) , 1 (track errors), 2 (verify), 3 (all).  The list of issues is printed to the standard error.");
            Console.Out.WriteLine("       -version=9 - sets the template to the passed version (9 in this example).");
            Console.Out.WriteLine("       encoding=UTF-8 (or other) - set BEFORE datasource to specify an encoding.");
            Console.Out.WriteLine("       locale=en_US - set the locale passed to the engine.");
            Console.Out.WriteLine("       pod=pod_filename - set a POD file (datasets).");
            Console.Out.WriteLine("       username=user password=pass - set BEFORE datasource for database connections.");
            Console.Out.WriteLine("       The datasource is identified with a pair of parameters");
            Console.Out.WriteLine("           -json filename - passes a JSON file as the datasource");
            Console.Out.WriteLine("               filename can be a url/filename or a connection string");
            Console.Out.WriteLine("           -odata url - passes a url as the datasource accessing it using the OData protocol");
            Console.Out.WriteLine("           -sforce - password should be password + security_token (passwordtoken)");
            Console.Out.WriteLine($"           -xml filename - XPath {CommandLine.DatasourceInfo.SAXON_XPATH_VERSION} passes an xml file as the datasource");
            Console.Out.WriteLine("                -xml xmlFilename=schema:schemaFilename - passes an xml file and a schema file as the datasource");
            Console.Out.WriteLine("               filename can be a url/filename or a connection string");
            Console.Out.WriteLine($"           -xpath filename - [deprecated] uses the old XPath {CommandLine.DatasourceInfo.LEGACY_XPATH_VERSION} datasource.");
            Console.Out.WriteLine("                -xml xmlFilename=schema:schemaFilename - passes an xml file and a schema file as the datasource");
            Console.Out.WriteLine("               filename can be a url/filename or a connection string");
            foreach (AdoDriverInfo di in AdoDriverInfo.AdoConnectors)
                Console.Out.WriteLine("           -" + di.Name + " connection_string ex: " + di.Example);
            Console.Out.WriteLine("               if a datasource is named you use the syntax -type:name (ex: -xml:name filename.xml)");
            Console.Out.WriteLine("       You can have 0-N key=value pairs that are passed to the datasource Map property");
            Console.Out.WriteLine("            If the value starts with I', F', or D' it parses it as an integer, float, or date(yyyy-MM-ddThh:mm:ss)");
            Console.Out.WriteLine("            If the value is * it will set a filter of all");
            Console.Out.WriteLine("            If the value is \"text,text,...\" it will set a filter of all");
        }
    }

    /// <summary>
    /// This class contains everything passed in the command line. It makes no calls to Windward Reports.
    /// </summary>
    internal class CommandLine
    {

        private byte[] templateFile;

        /// <summary>
        /// Create the object.
        /// </summary>
        /// <param name="templateFilename">The name of the template file.</param>
        /// <param name="reportFilename">The name of the report file. null for printer reports.</param>
        public CommandLine(string templateFilename, string reportFilename)
        {
            TemplateFilename = FileUtils.FullPath(templateFilename);
            if (!reportFilename.ToLower().EndsWith(".prn"))
                reportFilename = Path.GetFullPath(reportFilename);
            Launch = false;
            ReportFilename = reportFilename;
            Parameters = new Dictionary<string, object>();
            Datasources = new List<DatasourceInfo>();
            NumThreads = Environment.ProcessorCount * 2;
            VerifyFlag = 0;
        }

        public CommandLine(CommandLine src)
        {
            TemplateFilename = src.TemplateFilename;
            ReportFilename = src.ReportFilename;
            Parameters = src.Parameters == null ? null : new Dictionary<string, object>(src.Parameters);
            Datasources = src.Datasources == null ? null : new List<DatasourceInfo>(src.Datasources);
            DataProviders = src.DataProviders == null ? null : new Dictionary<string, IReportDataSource>(src.DataProviders);
            Locale = src.Locale;
            Launch = src.Launch;
            Cache = src.Cache;
            TemplateVersion = src.TemplateVersion;
            NumReports = src.NumReports;
            NumThreads = src.NumThreads;
            DataMode = src.DataMode;
            DataFileName = src.DataFileName;
            BaseDirectory = src.BaseDirectory;
            VerifyFlag = src.VerifyFlag;
            templateFile = src.templateFile?.ToArray();
        }

        /// <summary>
        /// The name of the template file.
        /// </summary>
        public string TemplateFilename { get; private set; }

        public Stream GetTemplateStream()
        {

            if ((FileUtils.GetFilesystemType(TemplateFilename) &
                (FileUtils.FilenameType.drive | FileUtils.FilenameType.unc)) == 0)
            {
                WebRequest request = WebRequest.Create(TemplateFilename);
                return request.GetResponse().GetResponseStream();
            }

            if (!Cache)
                return new FileStream(TemplateFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (templateFile == null)
                templateFile = File.ReadAllBytes(TemplateFilename);
            return new MemoryStream(templateFile);
        }

        /// <summary>
        /// The name of the report file. null for printer reports.
        /// </summary>
        public string ReportFilename { get; private set; }

        /// <summary>
        /// The output stream for the report.
        /// </summary>
        public Stream GetOutputStream()
        {
            if (!Cache)
            {
                if (!IsPerformance)
                    return new FileStream(ReportFilename, FileMode.Create, FileAccess.Write, FileShare.None);
                string dirReports = Path.GetDirectoryName(ReportFilename) ?? "";
                string extReport = ReportFilename.Substring(ReportFilename.LastIndexOf('.'));
                string filename = Path.GetFullPath(Path.Combine(dirReports, "rpt_" + Guid.NewGuid()) + extReport);
                return new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            }

            return new MemoryStream();
        }

        /// <summary>
        /// If we are caching the data providers, this is them for passes 1 .. N (set on pass 0)
        /// </summary>
        public IDictionary<string, IReportDataSource> DataProviders { get; set; }

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
        public Dictionary<string, object> Parameters { get; private set; }

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
        public bool IsPerformance
        {
            get { return NumReports != 0; }
        }

        /// <summary>
        /// Set to true to cache the template & datasources and write the output to a memory stream.
        /// </summary>
        public bool Cache { get; private set; }

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

        public int VerifyFlag { get; private set; }

        /// <summary>
        /// Base directory.
        /// </summary>
        public string BaseDirectory { get; private set; }

        /// <summary>
        /// The parameters passed for a single datasource. All filenames are expanded to full paths so that if an exception is
        /// thrown you know exactly where the file is.
        /// </summary>
        internal class DatasourceInfo
        {
            /// <summary>
            /// What type of datasource.
            /// </summary>
            internal enum TYPE
            {
                /// <summary>
                /// Use the REST protocol passing the credentials on the first request.
                /// </summary>
                REST,
                /// <summary>
                /// A SQL database.
                /// </summary>
                SQL,
                /// <summary>
                /// An XML file using Saxon.
                /// </summary>
                XML,
                /// <summary>
                /// An XML file using the .NET XPath library.
                /// </summary>
                XPATH_1,
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
                SFORCE,
                /// <summary>
                /// A data set
                /// </summary>
                DATA_SET
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
            private readonly string encoding;
            public bool restful;

            /// <summary>
            /// Create the object for a PLAYBACK datasource.
            /// </summary>
            /// <param name="filename">The playback filename.</param>
            /// <param name="type">What type of datasource.</param>
            public DatasourceInfo(string filename, TYPE type)
            {
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
            public DatasourceInfo(string name, string filename, string schemaFilename, string username, string password, string podFilename, TYPE type)
            {
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

            /// <summary>
            /// Create the object for a JSON datasource.
            /// </summary>
            /// <param name="name">The name for this datasource.</param>
            /// <param name="filename">The XML filename.</param>
            /// <param name="schemaFilename">The XML schema filename. null if no schema.</param>
            /// <param name="encoding">Theencoding of the JSON file.</param>
            /// <param name="type">What type of datasource.</param>
            public DatasourceInfo(string name, string filename, string schemaFilename, string encoding, TYPE type)
            {
                this.name = name ?? string.Empty;
                this.filename = GetFullPath(filename);
                if (!string.IsNullOrEmpty(schemaFilename))
                    this.schemaFilename = GetFullPath(schemaFilename);
                this.encoding = encoding;
                this.type = type;
            }

            /// <summary>
            /// Copy constructor. Does a deep copy.
            /// </summary>
            /// <param name="src">Initialize with the values in this object.</param>
		    public DatasourceInfo(DatasourceInfo src)
            {
                type = src.type;
                name = src.name;
                filename = src.filename;
                schemaFilename = src.schemaFilename;
                sqlDriverInfo = src.sqlDriverInfo;
                connectionString = src.connectionString;
                username = src.username;
                password = src.password;
                securitytoken = src.securitytoken;
                podFilename = src.podFilename;
                encoding = src.encoding;
                restful = src.restful;
            }

            // Saxon 10.1 provides XPath 3.1.
            public const string SAXON_XPATH_VERSION = "3.1";

            public const string LEGACY_XPATH_VERSION = "1.0";

            public string XPathVersion
            {
                get
                {
                    switch (type)
                    {
                        // Saxon XML implementation.  Update as needed when Saxon version changes.
                        case TYPE.XML:
                            return SAXON_XPATH_VERSION;

                        // The legacy XML implementation.  Likely not going to be changed.
                        case TYPE.XPATH_1:
                            return LEGACY_XPATH_VERSION;

                        // N/A for other datasources.
                        default:
                            return "";
                    }
                }
            }

            private static string GetFullPath(string filename)
            {
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
            public DatasourceInfo(string name, AdoDriverInfo sqlDriverInfo, string connectionString, string username, string password, string podFilename, TYPE type)
            {
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
            public TYPE Type
            {
                get { return type; }
            }

            /// <summary>
            /// The name for this datasource.
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// The XML or playback filename.
            /// </summary>
            public string Filename
            {
                get { return filename; }
            }

            /// <summary>
            /// The XML schema filename. null if no schema.
            /// </summary>
            public string SchemaFilename
            {
                get
                {
                    if (string.IsNullOrEmpty(schemaFilename) || schemaFilename.Contains("="))
                        return schemaFilename;

                    string connStr = updateConnectionStringProperty("", BaseAccessProvider.CONNECTION_URL, schemaFilename);
                    if (encoding != null)
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_ENCODING, encoding);
                    if (username != null)
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_USERNAME, username);
                    if (password != null)
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_PASSWORD, password);
                    if (restful)
                    {
                        TRAP.trap();
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_PROTOCOL, BaseAccessProvider.PROTOCOL_REST);
                        string accept = filename.ToLower().Contains(".json") ? global::net.windward.util.FileUtils.FILE_TYPE_JSON : global::net.windward.util.FileUtils.FILE_TYPE_XML;
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.HTTP_HEADER_MARKER + "Accept", accept);
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.HTTP_HEADER_MARKER + "Content-Type", accept);
                    }

                    return connStr;
                }
            }

            private static string updateConnectionStringProperty(String connectionString, String propertyName, String newValue)
            {

                // this way we create a new connection string.
                if (connectionString == null)
                    connectionString = "";

                // null means remove it
                if (newValue == null || newValue.length() == 0)
                    return removeConnectionStringProperty(connectionString, propertyName);

                StringBuilder buf = new StringBuilder();
                bool replacedIt = false;

                string[] tokens = connectionString.Split(';');

                foreach (string prop in tokens)
                {
                    String[] parts = prop.split("=");
                    if (parts[0].Equals(propertyName))
                    {
                        buf.Append(parts[0]).Append('=').Append(newValue).Append(';');
                        replacedIt = true;
                        continue;
                    }
                    buf.Append(prop).Append(';');
                }

                if (!replacedIt)
                    buf.Append(propertyName).Append('=').Append(newValue).Append(';');
                return buf.toString();
            }

            private static string removeConnectionStringProperty(String connectionString, String propertyName)
            {

                if (connectionString == null || connectionString.length() == 0)
                    return connectionString;

                StringBuilder buf = new StringBuilder();

                string[] tokens = connectionString.Split(';');
                foreach (string prop in tokens)
                {
                    String[] parts = prop.split("=");
                    if (parts[0].Equals(propertyName))
                        continue;
                    buf.Append(prop).Append(';');
                }
                return buf.toString();
            }

            /// <summary>
            /// The DriverInfo for the selected SQL vendor.
            /// </summary>
            public AdoDriverInfo SqlDriverInfo
            {
                get { return sqlDriverInfo; }
            }

            /// <summary>
            /// The connection string to connect to the database.
            /// </summary>
            public string ConnectionString
            {
                get { return connectionString; }
            }

            /// <summary>
            /// The connection string to connect to the database.
            /// </summary>
            public string ExConnectionString
            {
                get
                {
                    if (filename.Contains("="))
                        return filename;
                    string connStr = updateConnectionStringProperty("", BaseAccessProvider.CONNECTION_URL, filename);
                    if (encoding != null)
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_ENCODING, encoding);
                    if (username != null)
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_USERNAME, username);
                    if (password != null)
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_PASSWORD, password);
                    if (restful)
                    {
                        TRAP.trap();
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_PROTOCOL, BaseAccessProvider.PROTOCOL_REST);
                        string accept = filename.ToLower().Contains(".json") ? global::net.windward.util.FileUtils.FILE_TYPE_JSON : global::net.windward.util.FileUtils.FILE_TYPE_XML;
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.HTTP_HEADER_MARKER + "Accept", accept);
                        connStr = updateConnectionStringProperty(connStr, BaseAccessProvider.HTTP_HEADER_MARKER + "Content-Type", accept);
                    }
                    return connStr;
                }
            }

            /// <summary>
            /// The username if credentials are needed to access the datasource.
            /// </summary>
            public string Username
            {
                get { return username; }
            }

            /// <summary>
            /// The password if credentials are needed to access the datasource.
            /// </summary>
            public string Password
            {
                get { return password; }
            }

            /// <summary>
            /// The POD filename if datasets are being passed.
            /// </summary>
            public string PodFilename
            {
                get { return podFilename; }
            }

            public string Encoding
            {
                get { return encoding; }
            }
        }

        /// <summary>
        /// Create a CommandLine object from the command line passed to the program.
        /// </summary>
        /// <param name="args">The arguments passed to the program.</param>
        /// <returns>A CommandLine object populated from the args.</returns>
        public static CommandLine Factory(IList<string> args)
        {

            CommandLine rtn = new CommandLine(args[0], args[1]);

            string username = null, password = null, podFilename = null, encoding = null;

            for (int ind = 2; ind < args.Count; ind++)
            {
                string[] sa = args[ind].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                string cmd = sa[0].Trim();
                string name = sa.Length < 2 ? "" : sa[1].Trim();

                if (cmd == "-embed")
                {
                    rtn.DataMode = Report.DATA_MODE.ALL_ATTRIBUTES | Report.DATA_MODE.DATA | Report.DATA_MODE.EMBED;
                    continue;
                }

                if (cmd == "-data")
                {
                    rtn.DataMode = Report.DATA_MODE.ALL_ATTRIBUTES | Report.DATA_MODE.DATA;
                    rtn.DataFileName = Path.GetFullPath(args[++ind]);
                    continue;
                }

                if (cmd == "-performance")
                {
                    rtn.NumReports = int.Parse(name);
                    continue;
                }

                if (cmd == "-threads")
                {
                    rtn.NumThreads = int.Parse(name);
                    continue;
                }

                if (cmd == "-cache")
                {
                    rtn.Cache = true;
                    continue;
                }

                if (cmd == "-verify")
                {
                    rtn.VerifyFlag = int.Parse(name);
                    continue;
                }

                if (cmd == "-launch")
                {
                    rtn.Launch = true;
                    continue;
                }

                if (cmd == "-basedir")
                {
                    rtn.BaseDirectory = args[++ind];
                    continue;
                }

                if (cmd == "-rest")
                {
                    if (rtn.Datasources.Count > 0)
                        rtn.Datasources[rtn.Datasources.Count - 1].restful = true;
                    continue;
                }

                if ((cmd == "-xml") || (cmd == "-xpath"))
                {
                    string filename = args[++ind];
                    string schemaFilename = null;
                    int split = filename.IndexOf("=schema:");
                    if (split == -1)
                        schemaFilename = null;
                    else
                    {
                        schemaFilename = filename.Substring(split + 8).Trim();
                        filename = filename.Substring(0, split).Trim();
                    }
                    DatasourceInfo.TYPE type = (cmd == "-rest" ? DatasourceInfo.TYPE.REST
                        : (cmd == "-xpath" ? DatasourceInfo.TYPE.XPATH_1 : DatasourceInfo.TYPE.XML));
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, filename, schemaFilename, username, password, podFilename, type);
                    rtn.Datasources.Add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd == "-odata")
                {
                    string url = args[++ind];
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, null, username, password, podFilename, DatasourceInfo.TYPE.ODATA);
                    rtn.Datasources.Add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd == "-json")
                {
                    string url = args[++ind];
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, null, encoding, DatasourceInfo.TYPE.JSON);
                    rtn.Datasources.Add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd == "-dataset")
                {
                    string dataSetStr = args[++ind];
                    DatasourceInfo dsInfo = new DatasourceInfo(name, dataSetStr, null, null, DatasourceInfo.TYPE.DATA_SET);
                    rtn.Datasources.Add(dsInfo);
                    username = password = podFilename = null;
                    continue;
                }


                bool isDb = false;

                if (cmd == "-sforce")
                {
                    string url = "https://login.salesforce.com";
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, null, username, password, podFilename, DatasourceInfo.TYPE.SFORCE);
                    rtn.Datasources.Add(datasourceOn);
                    isDb = true;
                    username = password = podFilename = null;
                }
                foreach (AdoDriverInfo di in AdoDriverInfo.AdoConnectors)
                    if (cmd == "-" + di.Name)
                    {
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
                string[] keyValue = args[ind].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length != 2)
                    if (keyValue.Length == 1 && args[ind].EndsWith("="))
                    {
                        // We have a variable with the empty value.
                        keyValue = new string[2] { keyValue[0], "" };
                    }
                    else if (keyValue.Length < 2)
                    {
                        throw new ArgumentException($"Unknown option {args[ind]}.  If it's a SQL datasource, make sure the appropriate data provider is installed.");
                    }
                    else
                    {
                        // put the rest together.
                        string val = "";
                        for (int i = 1; i < keyValue.Length; i++)
                        {
                            val += keyValue[i];
                            if (i < keyValue.Length - 1)
                                val += '=';
                        }
                        keyValue = new string[2] { keyValue[0], val };
                    }

                switch (keyValue[0])
                {
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
                    case "encoding":
                        encoding = keyValue[1];
                        break;
                    default:
                        object value;
                        // may be a list
                        if (keyValue[1].IndexOf(',') != -1)
                        {
                            string[] tok = keyValue[1].Split(new char[] { ',' });
                            IList items = new ArrayList();
                            foreach (string elem in tok)
                                items.Add(ConvertValue(elem, rtn.Locale));
                            value = items;
                        }
                        else if (keyValue[1] == "*")
                            value = new FilterCondition(FilterBase.SORT_ORDER.NO_OVERRIDE);
                        else
                            value = ConvertValue(keyValue[1], rtn.Locale);
                        rtn.Parameters.Add(keyValue[0], value);
                        break;
                }
            }

            return rtn;
        }

        private static object ConvertValue(string keyValue, string locale)
        {
            if (keyValue.StartsWith("I'"))
                return Convert.ToInt64(keyValue.Substring(2));
            if (keyValue.StartsWith("F'"))
                return Convert.ToDouble(keyValue.Substring(2));
            if (keyValue.StartsWith("D'"))
                return Convert.ToDateTime(keyValue.Substring(2), (locale == null ? CultureInfo.CurrentCulture : new CultureInfo(locale)));
            return keyValue.Replace("\\n", "\n").Replace("\\t", "\t");
        }
    }

    internal class PerfCounters
    {
        internal TimeSpan timeSetup;
        internal TimeSpan timeApplyData;
        internal TimeSpan timeLayout;
        internal TimeSpan timeOutput;
        internal int numReports;
        internal int numPages;

        public void Add(PerfCounters pc)
        {
            timeSetup += pc.timeSetup;
            timeApplyData += pc.timeApplyData;
            timeLayout += pc.timeLayout;
            timeOutput += pc.timeOutput;
            numReports += pc.numReports;
            numPages += pc.numPages;
        }
    }

    /// <summary>
    /// Information on all known ADO.NET connectors.
    /// </summary>
    internal class AdoDriverInfo
    {
        private readonly string name;
        private readonly string classname;
        private readonly string example;

        /// <summary>
        /// Create the object for a given vendor.
        /// </summary>
        /// <param name="name">The -vendor part in the command line (ex: -sql).</param>
        /// <param name="classname">The classname of the connector.</param>
        /// <param name="example">A sample commandline.</param>
        public AdoDriverInfo(string name, string classname, string example)
        {
            this.name = name;
            this.classname = classname;
            this.example = example;
        }

        /// <summary>
        /// The -vendor part in the command line (ex: -sql).
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// The classname of the connector.
        /// </summary>
        public string Classname
        {
            get { return classname; }
        }

        /// <summary>
        /// A sample commandline.
        /// </summary>
        public string Example
        {
            get { return example; }
        }

        // can't add providers with these names - these are our ones
        private static string[] ourProviders = { "db2", "json", "mysql", "odbc", "odata", "oledb", "oracle", "postgresql", "sforce", "sharepoint", "sql", "xml", "xpath" };

        private static List<AdoDriverInfo> listProviders;

        internal static IList<AdoDriverInfo> AdoConnectors
        {
            get
            {
                if (listProviders != null)
                    return AdoDriverInfo.listProviders;
                listProviders = new List<AdoDriverInfo>();

                WrProviderFactories.ProviderInfo info = WrProviderFactories.GetAllProviders(false);
                foreach (WrVendor vendor in info.Vendors)
                {
                    // specific ones, we know the connection string syntax
                    switch (vendor.ProviderClass)
                    {
                        case "IBM.Data.DB2":
                            listProviders.Add(new AdoDriverInfo("db2", "IBM.Data.DB2", "server=db2.windwardreports.com;database=SAMPLE;Uid=demo;Pwd=demo;"));
                            break;
                        case "MySql.Data.MySqlClient":
                        case "MySql.Data.MySqlClient.MySqlClientFactory":
                            listProviders.Add(new AdoDriverInfo("mysql", vendor.ProviderClass, "server=mysql.windwardreports.com;database=sakila;user id=demo;password=demo;"));
                            break;
                        case "System.Data.Odbc":
                            listProviders.Add(new AdoDriverInfo("odbc", "System.Data.Odbc", "Driver={Sql Server};Server=localhost;Database=Northwind;User ID=test;Password=pass;"));
                            break;
                        case "System.Data.OleDb":
                            listProviders.Add(new AdoDriverInfo("oledb", "System.Data.OleDb", "Provider=sqloledb;Data Source=localhost;Initial Catalog=Northwind;User ID=test;Password=pass;"));
                            break;
                        case "Oracle.ManagedDataAccess.Client":
                            listProviders.Add(new AdoDriverInfo("oracle", "Oracle.ManagedDataAccess.Client", "Data Source=oracle.windwardreports.com:1521/HR;Persist Security Info=True;Password=HR;User ID=HR"));
                            break;
                        case "System.Data.SqlClient":
                            listProviders.Add(new AdoDriverInfo("sql", "System.Data.SqlClient", "Data Source=mssql.windwardreports.com;Initial Catalog=Northwind;user=demo;password=demo;"));
                            break;
                        case "Npgsql":
                            if (vendor is WrRedshiftVendor)
                                listProviders.Add(new AdoDriverInfo("redshift", "Npgsql", "HOST=localhost;DATABASE=pagila;USER ID=test;PASSWORD=test;"));
                            else
                                listProviders.Add(new AdoDriverInfo("postgresql", "Npgsql", "HOST=localhost;DATABASE=pagila;USER ID=test;PASSWORD=test;"));
                            break;

                        default:
                            // special case for DB2 - they put the version number in the classname
                            if (vendor.ProviderClass.StartsWith("IBM.Data.DB2."))
                            {
                                listProviders.Add(new AdoDriverInfo("db2", "IBM.Data.DB2", "server=localhost;database=SAMPLE;Uid=test;Pwd=pass;"));
                                continue;
                            }

                            string name = vendor.Name.ToLower();
                            if (name.Contains("deprecated"))
                                continue;
                            if (name.EndsWith("(cdata)"))
                                name = name.Substring(0, name.Length - 7).Trim();
                            name = name.Replace(' ', '_');
                            if (ourProviders.Any(op => op == name))
                                continue;

                            listProviders.Add(new AdoDriverInfo(name, vendor.ProviderClass, "connection_string"));
                            break;
                    }
                }

                listProviders.Sort((adi1, adi2) => adi1.Name.CompareTo(adi2.Name));
                return listProviders;
            }
        }
    }
}
