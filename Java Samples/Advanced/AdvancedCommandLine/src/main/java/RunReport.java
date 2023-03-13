/*
 * Copyright (c) 2011 by Windward Studios, Inc. All rights reserved.
 *
 * This program can be copied or used in any manner desired.
 */


import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.SelectBase;
import net.windward.datasource.SelectFilter;
import net.windward.datasource.dataset.DataSetDataSource;
import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.datasource.jdbc.JdbcDataSource;
import net.windward.datasource.json.JsonDataSource;
import net.windward.datasource.salesforce.SalesForceDataSource;
import net.windward.datasource.xml.SaxonDataSource;
import net.windward.env.SystemWrapper;
import net.windward.format.htm.HtmlImage;
import net.windward.util.AccessProviders.BaseAccessProvider;
import net.windward.util.StringUtils;
import net.windward.util.datetime.WindwardDateTime;
import net.windward.xmlreport.*;
import net.windward.xmlreport.errorhandling.Issue;
import org.apache.commons.io.FilenameUtils;
import org.apache.commons.io.IOUtils;
import org.apache.commons.logging.LogFactory;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.*;
import java.lang.reflect.Method;
import java.net.URL;
import java.sql.Driver;
import java.sql.DriverManager;
import java.text.DateFormat;
import java.text.ParsePosition;
import java.text.SimpleDateFormat;
import java.util.*;

/**
 * A sample usage of Windward Reports. This program generates reports based on the command line.
 * This project is used for two purposes, as a sample and as a way to run reports easily from the command line (mostly
 * for testing). The second ues provides one downside and one upside as a sample. The downside is it includes items like
 * the recorder that would not be included if this was solely as a sample. The upside is it does pretty much everything
 * because of the needs as a way to test any report.
 * <p/>
 * To get the parameters, the RunReport with no parameters and it will list them all out.
 */

public class RunReport {

    private static Logger log = LogManager.getLogger(RunReport.class);

    /**
     * Create a report using Windward Reports.
     *
     * @param args Run with no parameters to list out usage.
     * @throws Throwable Thrown if anything goes wrong.
     */
    public static void main(String[] args) throws Throwable {

        // if no arguments, then we list out the usage.
        if (args.length < 2) {
            DisplayUsage();
            return;
        }

        // parse the arguments passed in. This method makes no calls to Windward, it merely organizes the passed in arguments.
        CommandLine cmdLine = CommandLine.Factory(args);

        // the try here is so we can print out an exception if it is thrown. This code does minimal error checking and no other
        // exception handling to keep it simple & clear.
        try {

            // Initialize the reporting engine. This will throw an exception if the engine is not fully installed or you
            // do not have a valid license key in RunReport.exe.config.
            ProcessReport.init();

            // start elapsed after
            // init() as that only occurs on start-up and is not relevant in an app
            Date start = new Date();

            if (!cmdLine.isPerformance()) {
                PerfCounters perfCounters = runOneReport(cmdLine, args.length == 2);
                printPerformanceCounter(start, perfCounters, false);
            }
            else {
                new RunReport().runMultipleReports(cmdLine);
            }

            if (SystemWrapper.hasOpenDatabaseConnections()) {
                System.err.println("Warning: Did not close all database connections. Open connections written to log.");
                SystemWrapper.logOpenDatabaseConnections();
            }

            ProcessReport.shutdown();

        } catch (Throwable t) {
            log.debug("RunReport main()", t);
            if (SystemWrapper.hasOpenDatabaseConnections())
                SystemWrapper.logOpenDatabaseConnections();
            System.err.println();
            System.err.println("Error: " + t.getMessage());
            System.err.println("Stack trace:");
            t.printStackTrace();
            throw t;
        }
    }

    private void runMultipleReports(CommandLine cmdLine) throws InterruptedException, IOException {

        File dirReports = new File(cmdLine.getReportFilename()).getAbsoluteFile().getParentFile();
        if (!dirReports.isDirectory()) {
            System.err.println("The directory " + dirReports.getAbsolutePath() + " does not exist");
            return;
        }

        // load the template once (otherwise once per thread)
        if (cmdLine.isCache()) {
            InputStream stream = cmdLine.getTemplateStream();
            stream.close();
        }

        // drop out threads - default is twice the number of cores.
        int numThreads = cmdLine.getNumThreads();
        numReportsRemaining = cmdLine.getNumReports();

        // run num threads
        ReportWorker[] th = new ReportWorker[numThreads];
        for (int ind = 0; ind < numThreads; ind++)
            th[ind] = new ReportWorker(ind, new CommandLine(cmdLine));

        DateFormat df = DateFormat.getTimeInstance(DateFormat.MEDIUM);
        Date startTime = new Date();
        System.out.println("Start time: " + df.format(startTime) + ", " + numThreads + " threads, " + cmdLine.getNumReports() + " reports");

        for (int ind = 0; ind < numThreads; ind++)
            th[ind].start();

        // we wait
        synchronized (this) {
            threadsRunning += numThreads;
            while (threadsRunning > 0)
                wait();
        }

        PerfCounters perfCounters = new PerfCounters();
        for (int ind = 0; ind < numThreads; ind++)
            perfCounters.add(th[ind].perfCounters);

        System.out.println();
        printPerformanceCounter(startTime, perfCounters, true);
    }

    private static void printPerformanceCounter(Date startTime, PerfCounters perfCounters, boolean multiThreaded) {

        Date endTime = new Date();
        DateFormat df = DateFormat.getTimeInstance(DateFormat.MEDIUM);

        long elapsed = endTime.getTime() - startTime.getTime();
        System.out.println("End time: " + df.format(endTime));
        System.out.println("Elapsed time: " + ticksAsTime(elapsed));
        System.out.println("Time per report: " + (perfCounters.numReports == 0 ? "n/a" : ticksAsTime(elapsed / perfCounters.numReports)));
        System.out.println("Pages/report: " + (perfCounters.numReports == 0 ? "n/a" : perfCounters.numPages / perfCounters.numReports));
        System.out.printf("Pages/sec: %.02f", (float)(perfCounters.numPages * 1000L) / (float)elapsed).println();
        if (multiThreaded)
            System.out.println("Below values are totaled across all threads (and so add up to more than the elapsed time)");
        System.out.println("  Setup & parse: " + ticksAsTime(perfCounters.timeSetup));
        System.out.println("  Data: " + ticksAsTime(perfCounters.timeApplyData));
        System.out.println("  Layout: " + ticksAsTime(perfCounters.timeLayout));
        System.out.println("  Build output: " + ticksAsTime(perfCounters.timeOutput));
    }

    private static String ticksAsTime(long ticks) {

        int hours = (int) (ticks / (60 * 60 * 1000));
        ticks %= 60 * 60 * 1000;
        int minutes = (int) (ticks / (60 * 1000));
        ticks %= 60 * 1000;
        int seconds = (int) (ticks / 1000);
        ticks %= 1000;
        return twoDigits(hours) + ":" + twoDigits(minutes) + ":" + twoDigits(seconds) + "." + ticks;
    }

    private static String twoDigits(int time) {
        String rtn = Integer.toString(time);
        while (rtn.length() < 2)
            rtn = '0' + rtn;
        return rtn;
    }

    private int numReportsRemaining;

    private synchronized boolean hasNextReport() {
        numReportsRemaining--;
        return numReportsRemaining >= 0;
    }

    private int threadsRunning = 0;

    private synchronized void markDone() {
        threadsRunning--;
        notify();
    }

    private class ReportWorker extends Thread {
        private int threadNum;
        private CommandLine cmdLine;
        PerfCounters perfCounters;

        ReportWorker(int threadNum, CommandLine cmdLine) {
            this.threadNum = threadNum;
            this.cmdLine = cmdLine;
            perfCounters = new PerfCounters();
        }

        public void run() {

            try {
                while (hasNextReport()) {
                    System.out.print("" + threadNum + '.');
                    PerfCounters pc = runOneReport(cmdLine, false);
                    perfCounters.add(pc);
                }
            } catch (Exception e) {
                e.printStackTrace();
            } finally {
                markDone();
            }
        }
    }

    private static PerfCounters runOneReport(CommandLine cmdLine, boolean preservePodFraming) throws Exception {

        Date start = new Date();
        PerfCounters perfCounters = new PerfCounters();

        // get the template and output file streams. Output is null for printers
        InputStream template = cmdLine.getTemplateStream();
        OutputStream reportOutput;

        // if it's HTML and set to split pages, no reportOutput; ProcessHtml creates page streams
        if (isHTMLOutput(cmdLine.getReportFilename()) && ProcessHtml.isSplitPagesFromProperties()) {
            reportOutput = null;
        }
        else if ((!cmdLine.getReportFilename().endsWith(".prn"))) &&
                (!cmdLine.getReportFilename().endsWith(".eps")) && (!cmdLine.getReportFilename().endsWith(".bmp")) &&
                (!cmdLine.getReportFilename().endsWith(".gif")) && (!cmdLine.getReportFilename().endsWith(".jpg")) &&
                (!cmdLine.getReportFilename().endsWith(".png")) && (!cmdLine.getReportFilename().endsWith(".tif")) &&
                (!cmdLine.getReportFilename().endsWith(".jpeg")) && (!cmdLine.getReportFilename().endsWith(".tiff")))
            reportOutput = cmdLine.getOutputStream();
        else
            reportOutput = null;

        if (!cmdLine.isPerformance()) {
            System.out.println("Template: " + cmdLine.getTemplateFilename());
            System.out.println("Report: " + cmdLine.getReportFilename());
        }

        // Create the report object, based on the file extension
        ProcessReport report = createReport(cmdLine, template, reportOutput);

        if (!cmdLine.isPerformance()) {
            System.out.printf("Data processor: %s%n", report.getDataProcessorVersion());
        }

        report.setTrackErrors(cmdLine.getVerifyFlag());

        if (cmdLine.isBaseDirectorySet())
            report.setBaseDirectory(cmdLine.getBaseDirectory());
        else
            report.setBaseDirectory(new File(cmdLine.getTemplateFilename()).getParent());

        // if we are applying no datasources then we keep the POD framing in the generated report.
        if (preservePodFraming)
            report.setPreservePodFraming(true);
        // if we have a locale, we set it (used when applying datasources).
        if (cmdLine.getLocale() != null) {
            if (!cmdLine.isPerformance())
                System.out.println("Using locale: " + cmdLine.getLocale() + "(" + cmdLine.getLocale().getDisplayLanguage() + "_" + cmdLine.getLocale().getDisplayCountry() + ")");
            report.setLocale(cmdLine.getLocale());
        }
        if (cmdLine.getTemplateVersion() != 0)
            report.setTemplateVersion(cmdLine.getTemplateVersion());
        if (cmdLine.getWriteTags() != -1)
            report.setWriteTags(cmdLine.getWriteTags());

        FileOutputStream dataStream = null;
        if (cmdLine.getDataMode() != 0) {
            report.setDataMode(cmdLine.getDataMode());
            if (cmdLine.getDataFileName() != null) {
                dataStream = new FileOutputStream(cmdLine.getDataFileName());
                report.setDataStream(dataStream);
            }
        }

//        report.setModeProcessEmbedded(ProcessReport.MODE_PROCESS_EMBEDDED_COPY);
        // This first call parses the template and prepares the report so we can apply data to it.
        report.processSetup();

        Date postSetup = new Date();
        perfCounters.timeSetup = postSetup.getTime() - start.getTime();

        // list out vars
        if (cmdLine.getNumDatasources() > 0)
            for (Map.Entry entry : cmdLine.getMap().entrySet())
                System.out.println(entry.getKey() + " = " + entry.getValue() + (entry.getValue() == null ? "" : " (" + entry.getValue().getClass().getName() + ")"));

        Map<String, DataSourceProvider> dataProviders = null;
        if (cmdLine.isCache())
            dataProviders = cmdLine.getDataProviders();
        if (dataProviders == null)
            dataProviders = new HashMap<String, DataSourceProvider>();

        // Now for each datasource, we apply it to the report. This is complex because it handles all datasource types
        // as well as recording and playback.
        if (dataProviders.size() == 0)
            for (int ind = 0; ind < cmdLine.getNumDatasources(); ind++) {
                CommandLine.DatasourceInfo dsInfo = cmdLine.getDatasource(ind);

                // build the datasource
                DataSourceProvider datasource;
                InputStream dsStream = null;
                InputStream schemaStream = null;
                switch (dsInfo.getType()) {

                    case CommandLine.DatasourceInfo.TYPE_JSON:
                        if (!cmdLine.isPerformance())
                            System.out.println("JSON datasource: " + dsInfo.getFilename());

                        datasource = new JsonDataSource(dsInfo.getExConnectionString(), JsonDataSource.MODE_CONNECTION_STRING);
                        break;

                    // An XML datasource.
                    case CommandLine.DatasourceInfo.TYPE_XML:
                        if (!cmdLine.isPerformance()) {
                            printXPathInfo(dsInfo);
                        }
                        datasource = new SaxonDataSource(dsInfo.getExConnectionString(), dsInfo.getSchemaFilename());
                        break;

                    // using the old dom4j XPath 1.0
                    case CommandLine.DatasourceInfo.TYPE_DOM4J:
                        if (!cmdLine.isPerformance()) {
                            printXPathInfo(dsInfo);
                        }
                        datasource = new Dom4jDataSource(dsInfo.getExConnectionString(), dsInfo.getSchemaFilename());
                        break;

                    // An OData datasource.
                    case CommandLine.DatasourceInfo.TYPE_ODATA:
                        if (!cmdLine.isPerformance())
                            System.out.println("OData datasource: " + dsInfo.getFilename());

                        datasource = new net.windward.datasource.odata.ODataDataSource(dsInfo.getExConnectionString());

                        break;

                    //A SalesForce datsource.
                    case CommandLine.DatasourceInfo.TYPE_SFORCE:
                        if (!cmdLine.isPerformance())
                            System.out.println("SalesForce datasource: " + dsInfo.getFilename());
                        datasource = new SalesForceDataSource(dsInfo.username, dsInfo.password, "", true); //security token field is empty string because expected password input is password+security token
                        break;

                    case CommandLine.DatasourceInfo.TYPE_DATASET:
                        if (!cmdLine.isPerformance())
                            System.out.println("DATASET datasource: " + dsInfo.getFilename());
                        boolean datasetArgError = false;
                        String dataSetStr = dsInfo.getFilename();
                        String[] dataSetArgs = dataSetStr.split(";");
                        if(dataSetArgs.length != 2)
                            throw new IllegalAccessException("A the dataset string must be in the format \"ds=dataSourceName;select=Select\"");
                        String ds = null;
                        String select = null;
                        for(int i = 0; i < dataSetArgs.length; i++){
                            String[] argOn = dataSetArgs[i].split("=");
                            if(argOn.length != 2)
                                throw new IllegalAccessException("A the dataset string must be in the format \"ds=dataSourceName;select=Select\"");
                            if(argOn[0].equals("ds")){
                                ds = argOn[1];
                                continue;
                            }
                            if(argOn[0].equals("select")){
                                select = argOn[1];
                                continue;
                            }
                            throw new IllegalAccessException("A the dataset string must be in the format \"ds=dataSourceName;select=Select\"");
                        }

                        datasource = new DataSetDataSource(dsInfo.getName(), select, dataProviders.get(ds));
                        break;

                    case CommandLine.DatasourceInfo.TYPE_SQL:
                        String url = dsInfo.getConnectionString();
                        if (! url.startsWith(dsInfo.getSqlDriverInfo().getUrl()))
                            url = dsInfo.getSqlDriverInfo().getUrl() + url;
                        if (!cmdLine.isPerformance())
                            System.out.println(dsInfo.getSqlDriverInfo().getName() + " datasource: " + url);
                        datasource = new JdbcDataSource(dsInfo.getSqlDriverInfo().getDriver(),
                                url,
                                dsInfo.username, dsInfo.password);
                        break;
                    default:
                        throw new IllegalArgumentException("Unknown datasource type " + dsInfo.getType());
                }
                if (!cmdLine.isPerformance()) {
                    if (dsInfo.getUsername() != null)
                        System.out.println("    username=" + dsInfo.getUsername());
                    if (dsInfo.getPassword() != null)
                        System.out.println("    password=" + dsInfo.getPassword());
                    if (dsInfo.getPodFilename() != null)
                        System.out.println("    POD filename=" + dsInfo.getPodFilename());
                }

                dataProviders.put(dsInfo.getName(), datasource);

                // because of the switch above we have to explicitly close instead of structuring as a using{}
                if (dsStream != null)
                    dsStream.close();
                if (schemaStream != null)
                    schemaStream.close();
            }

        if (dataProviders.size() > 0) {
            report.setParameters(cmdLine.getMap());
            report.processData(dataProviders);
        }

        if (cmdLine.isCache())
            cmdLine.setDataProviders(dataProviders);
        else
            for(DataSourceProvider provider : dataProviders.values())
                provider.close();

        if (!cmdLine.isPerformance()) {
            System.out.println("All data applied, generating final report...");
        }

        Date postData = new Date();
        perfCounters.timeApplyData = postData.getTime() - postSetup.getTime();

        // Now that all the data has been applied, we generate the final output report. This does the
        // page layout and then writes out the output file.

        long layoutTime = report.processComplete();

        perfCounters.timeLayout = layoutTime;
        perfCounters.timeOutput = new Date().getTime() - postData.getTime() - layoutTime;
        perfCounters.numPages = report.getNumPages();
        perfCounters.numReports = 1;

        printVerify(report);

        // close out data.xml
        if (dataStream != null) {
            dataStream.close();
            System.out.println("data written to: " + cmdLine.getDataFileName());
        }

        // Close the output template to get everything written.
        template.close();
        if (reportOutput != null)
            reportOutput.close();

        // Close output streams for HTML -- don't worry if split_pages en/disabled
        if (report instanceof ProcessHtml && ((ProcessHtml) report).getPages() != null) {
            ArrayList<ByteArrayOutputStream> pages = ((ProcessHtml) report).getPages();
            String prefix = FilenameUtils.removeExtension(cmdLine.getReportFilename());
            String extension = FilenameUtils.getExtension(cmdLine.getReportFilename());
            for (int i = 0; i < pages.size(); ++i) {
                ByteArrayOutputStream page = pages.get(i);
                page.flush();
                String filename = prefix + "_" + Integer.toString(i) + "." + extension;
                page.writeTo(new FileOutputStream(filename));
                System.out.println("HTML page written to " + filename);
                page.close();
            }
        }

        // pages for images
        if (report instanceof ProcessImage) {
            ProcessImage procImage = (ProcessImage) report;
            if (procImage.getPages() != null) {
                String prefix = FilenameUtils.removeExtension(cmdLine.getReportFilename());
                String extension = FilenameUtils.getExtension(cmdLine.getReportFilename());
                for (int i = 0; i < procImage.getPages().size(); ++i) {
                    byte[] page = procImage.getPages().get(i);
                    String filename = prefix + "_" + Integer.toString(i) + "." + extension;
                    FileOutputStream stream = new FileOutputStream(filename);
                    stream.write(page);
                    System.out.println("Bitmap page written to " + filename);
                    stream.close();
                }
            }
        }

        if (!cmdLine.isPerformance())
            System.out.println("Report complete, " + report.getNumPages() + " pages long");
        report.close();

        if (cmdLine.isLaunch()) {
            String filename = cmdLine.getReportFilename();
            System.out.println("launching report " + filename);

            try {
                // java.awt.Desktop.getDesktop().open(new File(filename));
                Class classDesktop = Class.forName("java.awt.Desktop");
                Method method = classDesktop.getMethod("getDesktop", (Class[]) null);
                Object desktop = method.invoke(null, (Object[]) null);
                method = classDesktop.getMethod("open", File.class);
                method.invoke(desktop, new File(filename));

            } catch (Exception ex) {
                if (filename.indexOf(' ') != -1)
                    filename = '"' + filename + '"';
                String ver = System.getProperty("os.name");
                if (ver.toLowerCase().indexOf("windows") != -1)
                    Runtime.getRuntime().exec("rundll32 SHELL32.DLL,ShellExec_RunDLL " + filename);
                else
                    Runtime.getRuntime().exec("open " + filename);
            }
        }

        return perfCounters;
    }
    private static void printXPathInfo(CommandLine.DatasourceInfo dsInfo) {
        if (dsInfo.getSchemaFilename() == null || dsInfo.getSchemaFilename().length() == 0) {
            System.out.println(String.format("XML (XPath %s) datasource: %s", dsInfo.getXPathVersion(), dsInfo.getFilename()));
        } else {
            System.out.println(String.format("XML (XPath %s) datasource: %s, schema %s", dsInfo.getXPathVersion(), dsInfo.getFilename(), dsInfo.getSchemaFilename()));
        }
    }

    private static void printVerify(ProcessReportAPI report)
    {
        for (Issue issue : report.getErrorInfo().getErrors())
        {
            System.err.println(issue.getMessage());
        }
    }

    private static ProcessReport createReport(CommandLine cmdLine, InputStream template, OutputStream reportOutput) throws SetupException {
        if (isHTMLOutput(cmdLine.getReportFilename())) {
            return createHTMLReport(cmdLine.getReportFilename(), template, reportOutput);
        }

        if (cmdLine.getReportFilename().endsWith(".eps"))
            return new ProcessImage(template, HtmlImage.RENDER_EPS);
        if (cmdLine.getReportFilename().endsWith(".bmp"))
            return new ProcessImage(template, HtmlImage.BITMAP_BMP, 600);
        if (cmdLine.getReportFilename().endsWith(".gif"))
            return new ProcessImage(template, HtmlImage.BITMAP_GIF, 600);
        if (cmdLine.getReportFilename().endsWith(".jpg") || cmdLine.getReportFilename().endsWith(".jpeg"))
            return new ProcessImage(template, HtmlImage.BITMAP_JPG, 600);
        if (cmdLine.getReportFilename().endsWith(".png"))
            return new ProcessImage(template, HtmlImage.BITMAP_PNG, 600);
        if (cmdLine.getReportFilename().endsWith(".tif") || cmdLine.getReportFilename().endsWith(".tiff"))
            return new ProcessImage(template, HtmlImage.BITMAP_TIF, 600);

        if (cmdLine.getReportFilename().endsWith(".pdf")) {
            return new ProcessPdf(template, reportOutput);
        }
        if(cmdLine.getReportFilename().endsWith(".ps"))
            return new ProcessPostScript(template, reportOutput);
        if (cmdLine.getReportFilename().endsWith(".rtf"))
            return new ProcessRtf(template, reportOutput);
        if (cmdLine.getReportFilename().endsWith(".txt"))
            return new ProcessTxt(template, reportOutput);
        if (cmdLine.getReportFilename().endsWith(".docx") || cmdLine.getReportFilename().endsWith(".docm"))
            return new ProcessDocx(template, reportOutput);
        if (cmdLine.getReportFilename().endsWith(".xlsx") || cmdLine.getReportFilename().endsWith(".xlsm"))
            return new ProcessXlsx(template, reportOutput);
        if (cmdLine.getReportFilename().endsWith(".pptx") || cmdLine.getReportFilename().endsWith(".pptm"))
            return new ProcessPptx(template, reportOutput);
        if (cmdLine.getReportFilename().endsWith(".csv"))
            return new ProcessCsv(template, reportOutput);
        if (cmdLine.getReportFilename().endsWith(".prn")) {
            String printer;
            printer = cmdLine.getReportFilename().substring(0, cmdLine.getReportFilename().length() - 4);
            if (printer == null || printer.length() < 1)
                throw new IllegalArgumentException("no printer specified");
            return new ProcessPrinter(template, printer);
        }
        throw new IllegalArgumentException("output file must end with docx, docm, htm, html, pdf, pptx, pptm, rtf, txt, xhtml, xlsx, or xlsm");
    }

    private static void DisplayUsage() {
        System.out.println("Windward Reports version " + ProcessReport.getVersion());
        System.out.println("usage: RunReport template_file output_file [-basedir path] [-xml xml_file | -sql connection_string | -oracle connection_string | -ole oledb_connection_string] [licenseKey=value | ...]");
        System.out.println("       The template file can be a docx, pptx, or xlsx file.");
        System.out.println("       The output file extension determines the report type created:");
        System.out.println("           output.csv - SpreadSheet CSV file");
        System.out.println("           output.docx - Word 2007+ DOCX file");
        System.out.println("           output.htm - HTML file with no CSS");
        System.out.println("           output.html - HTML file with CSS");
        System.out.println("           output.pdf - Acrobat PDF file");
        System.out.println("           output.pptx - PowerPoint 2007+ PPTX file");
        System.out.println("           output.prn - Printer where \"output\" is the printer name");
        System.out.println("           output.rtf - Rich Text Format file");
        System.out.println("           output.txt - Ascii text file");
        System.out.println("           output.xhtml - XHTML file with CSS");
        System.out.println("           output.xlsx - Excel 2007+ XLSX file");
        System.out.println("           output.xlsm - Excel 2007+ macro enabled XLSM file");
        System.out.println("       -basedir c:\\test - sets the datasource base directory to the specified folder (c:\\test in this example)");
        System.out.println("       -data filename.xml - will write data.xml to this filename.");
        System.out.println("       -embed - will embed data.xml in the generated report. DOCX, PDF, PPTX, & XLSX only.");
        System.out.println("       -launch - will launch the report when complete.");
        System.out.println("       -performance:123 - will run the report 123 times.");
        System.out.println("            output file is used for directory and extension for reports");
        System.out.println("       -cache - will cache template & datasources, will write output to memory stream. Only used with -performance");
        System.out.println("       -threads:4 - will create 4 threads when running -performance.");
        System.out.println("       -verify:N - turn on the error handling and verify feature where N is a number: 0 (none) , 1 (track errors), 2 (verify), 3 (all).  The list of issues is printed to the standard error.");
        System.out.println("       -version=9 - sets the template to the passed version (9 in this example)");
        System.out.println("       encoding=UTF-8 (or other) - set BEFORE datasource to specify an encoding");
        System.out.println("       locale=en_US - set the locale passed to the engine.");
        System.out.println("       pod=pod_filename - set a POD file (datasets)");
        System.out.println("       username=user password=pass - set BEFORE datasource for database connections");
        System.out.println("       The datasource is identified with a pair of parameters (the [prepend] part is prepended to the connection string");
        for (int ind = 0; ind < JdbcDriverInfo.getDrivers().size(); ind++)
            System.out.println("           -" + JdbcDriverInfo.getDrivers().get(ind).getName() + " connection_string - ex: ["
                    + JdbcDriverInfo.getDrivers().get(ind).getUrl() + "]" + JdbcDriverInfo.getDrivers().get(ind).getExample());
        System.out.println("           -json filename - passes a JSON file as the datasource");
        System.out.println("                filename can be a url/filename or a connection string");
        System.out.println("           -odata url - passes a url as the datasource accessing it using the OData protocol");
        System.out.println("           -sforce - password should be password+securitytoken");
        System.out.println(String.format("           -xml filename - XPath %s passes an xml file as the datasource", CommandLine.DatasourceInfo.SAXON_XPATH_VERSION));
        System.out.println("                -xml xmlFilename=schema:schemaFilename - passes an xml file and a schema file as the datasource");
        System.out.println("                filename can be a filename or a connection string");
        System.out.println(String.format("           -dom4j filename - [deprecated] uses the old XPath %s datasource", CommandLine.DatasourceInfo.LEGACY_XPATH_VERSION));
        System.out.println("                -dom4j xmlFilename=schema:schemaFilename - passes an xml file and a schema file as the datasource");
        System.out.println("                filename can be a filename or a connection string");
        System.out.println("           -[xml|sql|...]:name names this datasource with name");
        System.out.println("                     must come BEFORE each -xml, -sql, ... part");
        System.out.println("       You can have 0-N key=value pairs that are passed to the datasource Map property");
        System.out.println("            If the value starts with I', F', or D' it parses it as an integer, float, or date(yyyy-MM-ddThh:mm:ss)");
        System.out.println("                example  date=\"D'1996-08-29\"");
        System.out.println("            If the value is * it will set a filter of all");
        System.out.println("            If the value is \\\"text,text,...\\\" it will set a filter of all");
    }

    /**
     * This class contains everything passed in the command line. It makes no calls to Windward Reports.
     */
    private static class CommandLine {
        private String templateFilename;
        private String reportFilename;
        private Map<String, Object> map;
        private List<DatasourceInfo> datasources;
        private Map<String, DataSourceProvider> dataProviders;
        private Locale locale;
        private boolean launch;
        private boolean cache;
        private int templateVersion;
        private int numReports;
        private int writeTags;
        private int numThreads;
        private int dataMode;
        private String dataFileName;
        private String baseDirectory;
        private int verifyFlag;
        private byte[] templateFile;

        /**
         * Create the object.
         *
         * @param templateFilename The name of the template file.
         * @param reportFilename   The name of the report file. null for printer reports.
         */
        public CommandLine(String templateFilename, String reportFilename) {
            this.templateFilename = GetFullPath(templateFilename);
            if (!reportFilename.toLowerCase().endsWith(".prn"))
                reportFilename = GetFullPath(reportFilename);
            launch = false;
            this.reportFilename = reportFilename;
            map = new HashMap<String, Object>();
            datasources = new ArrayList<DatasourceInfo>();
            writeTags = -1;
            numThreads = Runtime.getRuntime().availableProcessors() * 2;
            verifyFlag = ProcessReportAPI.ERROR_HANDLING_NONE;
        }

        private static String GetFullPath(String filename) {
            int pos = filename.indexOf(':');
            if ((pos == -1) || (pos == 1))
                return new File(filename).getAbsolutePath();
            return filename;
        }

        public CommandLine(CommandLine src) {
            templateFilename = src.templateFilename;
            reportFilename = src.reportFilename;
            map = src.map == null ? null : new HashMap<String, Object>(src.map);
            datasources = src.datasources == null ? null : new ArrayList<DatasourceInfo>(src.datasources);
            dataProviders = src.dataProviders == null ? null : new HashMap<String, DataSourceProvider>();
            locale = src.locale;
            launch = src.launch;
            cache = src.cache;
            templateVersion = src.templateVersion;
            numReports = src.numReports;
            writeTags = src.writeTags;
            numThreads = src.numThreads;
            dataMode = src.dataMode;
            dataFileName = src.dataFileName;
            baseDirectory = src.baseDirectory;
            verifyFlag = src.verifyFlag;
            templateFile = src.templateFile;
        }

        /**
         * The name of the template file.
         *
         * @return The name of the template file.
         */
        public String getTemplateFilename() {
            return templateFilename;
        }

        public InputStream getTemplateStream() throws IOException {

            int pos = templateFilename.indexOf(':');
            if ((pos != -1) && (pos != 1))
                return new URL(templateFilename).openStream();
            if (! cache)
                return new FileInputStream(templateFilename);

            if (templateFile == null) {
                FileInputStream stream = new FileInputStream(templateFilename);
                templateFile =  IOUtils.toByteArray(stream);
                stream.close();
            }
            return new ByteArrayInputStream(templateFile);
        }

        /**
         * The name of the report file. null for printer reports.
         *
         * @return The name of the report file. null for printer reports.
         */
        public String getReportFilename() {
            return reportFilename;
        }

        public OutputStream getOutputStream() throws IOException {
            if (! cache) {
                if (! isPerformance())
                    return new FileOutputStream(reportFilename);
                File dirReports = new File(reportFilename).getAbsoluteFile().getParentFile();
                String extReport = reportFilename.substring(reportFilename.lastIndexOf('.'));
                String filename = File.createTempFile("rpt_", extReport, dirReports).getAbsolutePath();
                return new FileOutputStream(filename);
            }

            return new ByteArrayOutputStream();
        }

        /**
         * The parameters passed for each datasource to be created.
         *
         * @return The parameters passed for each datasource to be created.
         */
        public List<DatasourceInfo> getDatasources() {
            return datasources;
        }

        /**
         * The parameters passed for each datasource to be created.
         *
         * @param datasources The parameters passed for each datasource to be created.
         */
        public void setDatasources(List<DatasourceInfo> datasources) {
            this.datasources = datasources;
        }

        /**
         * If we are caching the data providers, this is them for passes 1 .. N (set on pass 0)
         */
        public Map<String, DataSourceProvider> getDataProviders() {
            return dataProviders;
        }

        /**
         * If we are caching the data providers, this is them for passes 1 .. N (set on pass 0)
         */
        public void setDataProviders(Map<String, DataSourceProvider> dataProviders) {
            this.dataProviders = dataProviders;
        }

        /**
         * true if launch the app at the end.
         *
         * @return true if launch the app at the end.
         */
        public boolean isLaunch() {
            return launch;
        }

        /**
         * true if launch the app at the end.
         *
         * @param launch true if launch the app at the end.
         */
        public void setLaunch(boolean launch) {
            this.launch = launch;
        }

        /**
         * The template version number. 0 if not set.
         *
         * @return The template version number.
         */
        public int getTemplateVersion() {
            return templateVersion;
        }

        /**
         * The template version number. 0 if not set.
         *
         * @param templateVersion The template version number.
         */
        public void setTemplateVersion(int templateVersion) {
            this.templateVersion = templateVersion;
        }

        /**
         * The ProcessReportAPI.TAG_STYLE_* to write unhandled tags with.
         *
         * @param tagStyle ProcessReportAPI.TAG_STYLE_*
         */
        public void setWriteTags(int tagStyle) {
            this.writeTags = tagStyle;
        }

        /**
         * Gets the ProcessReportAPI.TAG_STYLE_* to write unhandled tags with, or -1 if not set.
         *
         * @return ProcessReportAPI.TAG_STYLE_* or -1 if not set
         */
        public int getWriteTags() {
            return writeTags;
        }

        /**
         * The name/value pairs for variables passed to the datasources. key is a String and value is a String,
         * Number, or Date.
         *
         * @return The name/value pairs for variables passed to the datasources.
         */
        public Map<String, Object> getMap() {
            return map;
        }

        /**
         * The number of datasources.
         *
         * @return The number of datasources.
         */
        public int getNumDatasources() {
            return datasources.size();
        }

        /**
         * The parameters passed for each datasource to be created.
         *
         * @param index The datasource to return.
         * @return The parameters for the datasource to be created.
         */
        public DatasourceInfo getDatasource(int index) {
            return datasources.get(index);
        }

        /**
         * The locale to run under.
         *
         * @return The locale to run under.
         */
        public Locale getLocale() {
            return locale;
        }

        /**
         * For performance modeling, how many reports to run.
         *
         * @return How many reports to run.
         */
        public int getNumReports() {
            return numReports;
        }

        /**
         * true if requesting a performance run
         *
         * @return true if requesting a performance run
         */
        public boolean isPerformance() {
            return numReports != 0;
        }

        /**
         * Set to true to cache the template & datasources and write the output to a memory stream.
         */
        public boolean isCache() {
            return cache;
        }

        /**
         * Set to true to cache the template & datasources and write the output to a memory stream.
         */
        public void setCache(boolean cache) {
            this.cache = cache;
        }

        /**
         * The number of threads to launch if running a performance test.
         *
         * @return The number of threads to launch if running a performance test.
         */
        public int getNumThreads() {
            return numThreads;
        }

        /**
         * The data mode for this report. Controls the generation of a data.xml file.
         *
         * @return The data mode for this report.
         */
        public int getDataMode() {
            return dataMode;
        }

        /**
         * If the data.xml is to be written to an external file, this is the file.
         *
         * @return If the data.xml is to be written to an external file, this is the file.
         */
        public String getDataFileName() {
            return dataFileName;
        }

        public String getBaseDirectory() {
            return baseDirectory;
        }

        public boolean isBaseDirectorySet() {
            return baseDirectory != null;
        }

        int getVerifyFlag() {
            return verifyFlag;
        }

        /**
         * The parameters passed for a single datasource. All filenames are expanded to full paths so that if an exception is
         * thrown you know exactly where the file is.
         */
        private static class DatasourceInfo {

            /**
             * A SQL database.
             */
            public static final int TYPE_SQL = 1;

            /**
             * An XML file.
             */
            public static final int TYPE_XML = 2;

            /**
             * An OData url.
             */
            public static final int TYPE_ODATA = 3;

            /**
             * JSON data source.
             */
            public static final int TYPE_JSON = 5;

            /**
             * SalesForce dat source.
             */
            public static final int TYPE_SFORCE = 6;

            /**
             * An XML file using dom4j (XPath 1.0)
             */
            public static final int TYPE_DOM4J = 7;

            public static final int TYPE_DATASET = 8;

            // Saxon 10.1 provides XPath 3.1.
            public static final String SAXON_XPATH_VERSION = "3.1";

            public static final String LEGACY_XPATH_VERSION = "1.0";

            private int type;
            private String name;

            private String filename;
            private String schemaFilename;

            private JdbcDriverInfo sqlDriverInfo;
            private String connectionString;

            private String username;
            private String password;
            private String podFilename;

            private String encoding;
            private boolean restful;

            /**
             * Create the object for a PLAYBACK datasource.
             *
             * @param filename The playback filename.
             * @param type     What type of datasource.
             */
            public DatasourceInfo(String filename, int type) {
                this.type = type;
                this.filename = filename;
            }

            /**
             * Create the object for a XML datasource.
             *
             * @param name           The name for this datasource.
             * @param filename       The XML filename.
             * @param schemaFilename The XML schema filename. null if no schema.
             * @param username       The username if credentials are needed to access the datasource.
             * @param password       The password if credentials are needed to access the datasource.
             * @param podFilename    The POD filename if datasets are being passed.
             * @param type           What type of datasource.
             */
            public DatasourceInfo(String name, String filename, String schemaFilename, String username, String password, String podFilename, int type) {
                this.name = name == null ? "" : name;
                this.filename = GetFullPath(filename);
                if ((schemaFilename != null) && (schemaFilename.length() > 0))
                    this.schemaFilename = GetFullPath(schemaFilename);
                this.username = username;
                this.password = password;
                if ((podFilename != null) && (podFilename.length() > 0))
                    this.podFilename = GetFullPath(podFilename);
                this.type = type;
            }

            /**
             * Create the object for an OData datasource.
             *
             * @param name        The name for this datasource.
             * @param url         the url for the service.
             * @param username    The username if credentials are needed to access the datasource.
             * @param password    The password if credentials are needed to access the datasource.
             * @param podFilename The POD filename if datasets are being passed.
             * @param type        What type of datasource.
             */
            public DatasourceInfo(String name, String url, String username, String password, String podFilename, int type) {
                this.name = name == null ? "" : name;
                this.filename = url;
                this.username = username;
                this.password = password;
                if ((podFilename != null) && (podFilename.length() > 0))
                    this.podFilename = GetFullPath(podFilename);
                this.type = type;
            }

            /**
             * Create the object for a JSON datasource.
             *
             * @param name        The name for this datasource.
             * @param url         the url for the service.
             * @param username    The username if credentials are needed to access the datasource.
             * @param password    The password if credentials are needed to access the datasource.
             * @param podFilename The POD filename if datasets are being passed.
             * @param type        What type of datasource.
             */
            public DatasourceInfo(String name, String url, String schema, String username, String password, String podFilename, String encoding, int type) {
                this.name = name == null ? "" : name;
                this.filename = url;
                this.username = username;
                this.password = password;
                if ((podFilename != null) && (podFilename.length() > 0))
                    this.podFilename = GetFullPath(podFilename);
                this.encoding = encoding;
                this.type = type;
            }

            /**
             * Create the object for a SQL datasource.
             *
             * @param name             The name for this datasource.
             * @param sqlDriverInfo    The DriverInfo for the selected SQL vendor.
             * @param connectionString The connection string to connect to the database.
             * @param username         The username if credentials are needed to access the datasource.
             * @param password         The password if credentials are needed to access the datasource.
             * @param podFilename      The POD filename if datasets are being passed.
             * @param type             What type of datasource.
             */
            public DatasourceInfo(String name, JdbcDriverInfo sqlDriverInfo, String connectionString, String username, String password, String podFilename, int type) {
                this.name = name;
                this.sqlDriverInfo = sqlDriverInfo;
                this.connectionString = connectionString;
                this.username = username;
                this.password = password;
                if ((podFilename != null) && (podFilename.length() > 0))
                    this.podFilename = GetFullPath(podFilename);
                this.type = type;
            }

            /**
             * What type of datasource.
             *
             * @return What type of datasource.
             */
            public int getType() {
                return type;
            }

            /**
             * The name for this datasource.
             *
             * @return The name for this datasource.
             */
            public String getName() {
                return name;
            }

            /**
             * The XML filename.
             *
             * @return The XML filename.
             */
            public String getFilename() {
                return filename;
            }

            /**
             * The XML schema filename. null if no schema.
             *
             * @return The XML schema filename. null if no schema.
             */
            public String getSchemaFilename() {

                if (schemaFilename == null || schemaFilename.length() == 0 || schemaFilename.contains("="))
                    return schemaFilename;

                String connStr = StringUtils.updateConnectionStringProperty("", BaseAccessProvider.CONNECTION_URL, schemaFilename);
                if (encoding != null)
                    connStr =  StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_ENCODING, encoding);
                if (username != null)
                    connStr =  StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_USERNAME, username);
                if (password != null)
                    connStr =  StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_PASSWORD, password);
                if (restful) {
                    connStr = StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_PROTOCOL, BaseAccessProvider.PROTOCOL_REST);
                    String accept = filename.toLowerCase().contains(".json") ? "json" : "xml";
                    connStr = StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.HTTP_HEADER_MARKER + "Accept", accept);
                    connStr = StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.HTTP_HEADER_MARKER + "Content-Type", accept);
                }

                return connStr;
            }

            /**
             * The DriverInfo for the selected SQL vendor.
             *
             * @return The DriverInfo for the selected SQL vendor.
             */
            public JdbcDriverInfo getSqlDriverInfo() {
                return sqlDriverInfo;
            }

            /**
             * The connection string to connect to the database.
             *
             * @return The connection string to connect to the database.
             */
            public String getConnectionString() {
                return connectionString;
            }

            /**
             * The username if credentials are needed to access the datasource.
             *
             * @return The username if credentials are needed to access the datasource.
             */
            public String getUsername() {
                return username;
            }

            /**
             * The password if credentials are needed to access the datasource.
             *
             * @return The password if credentials are needed to access the datasource.
             */
            public String getPassword() {
                return password;
            }

            /**
             * The POD filename if datasets are being passed.
             *
             * @return The POD filename if datasets are being passed.
             */
            public String getPodFilename() {
                return podFilename;
            }

            /**
             * The JSON encoding
             */
            public String getEncoding() {
                return encoding;
            }

            /**
             * The connection string the new way for XML, etc.
             */
            public String getExConnectionString () {
                if (filename.contains("="))
                    return filename;
                String connStr = StringUtils.updateConnectionStringProperty("", BaseAccessProvider.CONNECTION_URL, filename);
                if (encoding != null)
                    connStr =  StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_ENCODING, encoding);
                if (username != null)
                    connStr =  StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_USERNAME, username);
                if (password != null)
                    connStr =  StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_PASSWORD, password);
                if (restful) {
                    connStr = StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.CONNECTION_PROTOCOL, BaseAccessProvider.PROTOCOL_REST);
                    String accept = filename.toLowerCase().contains(".json") ? "json" : "xml";
                    connStr = StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.HTTP_HEADER_MARKER + "Accept", accept);
                    connStr = StringUtils.updateConnectionStringProperty(connStr, BaseAccessProvider.HTTP_HEADER_MARKER + "Content-Type", accept);
                }

                return connStr;
            }

            public String getXPathVersion() {
                switch (type) {
                    // Saxon XML implementation.  Update as needed when Saxon version changes.
                    case TYPE_XML:
                        return SAXON_XPATH_VERSION;

                    // The legacy XML implementation.  Likely not going to be changed.
                    case TYPE_DOM4J:
                        return LEGACY_XPATH_VERSION;

                    // N/A for other datasources.
                    default:
                        return "";
                }
            }
        }

        /**
         * Create a CommandLine object from the command line passed to the program.
         *
         * @param args The arguments passed to the program.
         * @return A CommandLine object populated from the args.
         */
        public static CommandLine Factory(String[] args) {

            CommandLine rtn = new CommandLine(args[0], args[1]);

            String username = null, password = null, podFilename = null, encoding = null;

            for (int ind = 2; ind < args.length; ind++) {
                int pos = args[ind].indexOf(':');
                String name = pos == -1 ? "" : args[ind].substring(pos + 1);
                String cmd = pos == -1 ? args[ind] : args[ind].substring(0, pos);

                if (cmd.equals("-embed")) {

                    rtn.dataMode = ProcessReport.DATA_MODE_ALL_ATTRIBUTES | ProcessReport.DATA_MODE_DATA | ProcessReport.DATA_MODE_EMBED;
                    continue;
                }

                if (cmd.equals("-data")) {

                    rtn.dataMode = ProcessReport.DATA_MODE_ALL_ATTRIBUTES | ProcessReport.DATA_MODE_DATA;
                    rtn.dataFileName = args[++ind];
                    continue;
                }

                if (cmd.equals("-performance")) {
                    rtn.numReports = Integer.parseInt(name);
                    continue;
                }

                if (cmd.equals("-threads")) {
                    rtn.numThreads = Integer.parseInt(name);
                    continue;
                }

                if (cmd.equals("-cache")) {
                    rtn.setCache(true);
                    continue;
                }

                if (cmd.equals("-verify")) {
                    rtn.verifyFlag = Integer.parseInt(name);
                    continue;
                }

                if (cmd.equals("-launch")) {
                    rtn.setLaunch(true);
                    continue;
                }

                if (cmd.equals("-basedir")) {
                    rtn.baseDirectory = args[++ind];
                    continue;
                }

                if (cmd.equals("-rest")) {
                    if (rtn.datasources.size() > 0)
                        rtn.datasources.get(rtn.datasources.size() - 1).restful = true;
                    continue;
                }

                if (cmd.equals("-xml") || cmd.equals("-dom4j")) {
                    String xmlFilename = args[++ind];
                    int split = xmlFilename.indexOf("=schema:");
                    String schemaFilename;
                    if (split == -1)
                        schemaFilename = null;
                    else {
                        schemaFilename = xmlFilename.substring(split + 8).trim();
                        xmlFilename = xmlFilename.substring(0, split).trim();
                    }
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, xmlFilename, schemaFilename, username, password, podFilename,
                            cmd.equals("-dom4j") ? DatasourceInfo.TYPE_DOM4J : DatasourceInfo.TYPE_XML);
                    rtn.datasources.add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd.equals("-json")) {
                    String url = args[++ind];
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, null, username, password, podFilename, encoding, DatasourceInfo.TYPE_JSON);
                    rtn.datasources.add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd.equals("-odata")) {
                    String url = args[++ind];
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, username, password, podFilename, DatasourceInfo.TYPE_ODATA);
                    rtn.datasources.add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd.equals("-sforce")) {
                    String url = "https://login.salesforce.com";
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, null, username, password, podFilename, DatasourceInfo.TYPE_SFORCE);
                    rtn.datasources.add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if(cmd.equals("-dataset")){
                    String dataSetStr = args[++ind];
                    DatasourceInfo dsInfo = new DatasourceInfo(name,dataSetStr,null,null,null, DatasourceInfo.TYPE_DATASET);
                    rtn.datasources.add(dsInfo);
                    username = password = podFilename = null;
                    continue;
                }
                boolean isDb = false;
                for (int index = 0; index < JdbcDriverInfo.getDrivers().size(); index++) {
                    JdbcDriverInfo di = JdbcDriverInfo.getDrivers().get(index);
                    if (cmd.equals("-" + di.getName())) {
                        DatasourceInfo datasourceOn = new DatasourceInfo(name, di, args[++ind], username, password, podFilename, DatasourceInfo.TYPE_SQL);
                        rtn.datasources.add(datasourceOn);
                        isDb = true;
                        username = password = podFilename = null;
                        break;
                    }
                }
                if (isDb)
                    continue;

                // assume this is a key=value
                int equ = args[ind].indexOf('=');
                if (equ == -1) {
                    throw new IllegalArgumentException("Unknown option " + args[ind] + ". If the option is a datasource driver related, make sure you have an appropriate driver's JAR files in CLASSPATH.");
                }

                String key = args[ind].substring(0, equ);
                String value = args[ind].substring(equ + 1);

                // locale is global
                if (key.equals("locale")) {
                    rtn.locale = new Locale(value.substring(0, 2), value.substring(3));
                    continue;
                }
                if (key.equals("version")) {
                    rtn.setTemplateVersion(Integer.parseInt(value));
                    continue;
                }

                if (key.equals("username")) {
                    username = value;
                    continue;
                }
                if (key.equals("password")) {
                    password = value;
                    continue;
                }
                if (key.equals("pod")) {
                    podFilename = value;
                    continue;
                }
                if (key.equals("encoding")) {
                    encoding = value;
                    continue;
                }
                if (key.equals("writeTags")) {
                    value = value.toLowerCase().trim();
                    switch (value) {
                        case "text":
                            rtn.setWriteTags(ProcessReportAPI.TAG_STYLE_TEXT);
                            break;
                        case "fields":
                            rtn.setWriteTags(ProcessReport.TAG_STYLE_FIELD);
                            break;
                        case "fields2007 (fields + objects)":
                            rtn.setWriteTags(ProcessReport.TAG_STYLE_FIELD_2007);
                            break;
                        default:
                            System.err.println("Warning: unrecognized value for writeTags key. Valid values are: text, fields, fields2007, controls.");
                            break;
                    }
                }

                Object val;
                // may be a list
                if (value.indexOf(',') != -1) {
                    ArrayList<Object> list = new ArrayList<>();
                    StringTokenizer tok = new StringTokenizer(value, ",", false);
                    while (tok.hasMoreTokens()) {
                        String elem = tok.nextToken();
                        list.add(convertValue(elem));
                    }
                    val = list;
                } else if (value.equals("*"))
                    val = new SelectFilter(SelectBase.SORT_NO_OVERRIDE);
                else
                    val = convertValue(value);
                rtn.map.put(key, val);
            }
            return rtn;
        }

        private static Object convertValue(String value) {

            if (value.startsWith("I'"))
                return Long.valueOf(value.substring(2));
            if (value.startsWith("F'"))
                return Double.valueOf(value.substring(2));
            if (value.startsWith("D'")) {
                ParsePosition pp = new ParsePosition(0);
                SimpleDateFormat stdFmt = new SimpleDateFormat("yyyy-MM-dd'T'hh:mm:ss");
                Date date = stdFmt.parse(value.substring(2), pp);
                if ((date != null) && (pp.getIndex() > 0))
                    return new WindwardDateTime(date);
                stdFmt = new SimpleDateFormat("yyyy-MM-dd");
                date = stdFmt.parse(value.substring(2), pp);
                if ((date != null) && (pp.getIndex() > 0))
                    return new WindwardDateTime(date);
                throw new IllegalArgumentException("Could not parse yyyy-MM-dd[Thh:mm:ss] date from " + value.substring(2));
            }
            // turn \n and \t into a true \n and \t
            return value.replace("\\n", "\n").replace("\\t", "\t");
        }
    }

    private static class PerfCounters {
        long timeSetup;
        long timeApplyData;
        long timeLayout;
        long timeOutput;
        int numReports;
        int numPages;

        public void add(PerfCounters pc) {
            timeSetup += pc.timeSetup;
            timeApplyData += pc.timeApplyData;
            timeLayout += pc.timeLayout;
            timeOutput += pc.timeOutput;
            numReports += pc.numReports;
            numPages += pc.numPages;
        }
    }

    /**
     * Information on all known JDBC connectors.
     */
    private static class JdbcDriverInfo {
        private String name;
        private String driver;
        private String url;
        private String example;

        /**
         * Create the object for a given vendor.
         *
         * @param name    The -vendor part in the command line (ex: -sql).
         * @param driver  The driver classname.
         * @param url     The url start used for this driver.
         * @param example A sample commandline.
         */
        public JdbcDriverInfo(String name, String driver, String url, String example) {
            this.name = name;
            this.driver = driver;
            this.url = url;
            this.example = example;
        }

        /**
         * The -vendor part in the command line (ex: -sql).
         *
         * @return The -vendor part in the command line (ex: -sql).
         */
        public String getName() {
            return name;
        }

        /**
         * The driver classname.
         *
         * @return The driver classname.
         */
        public String getDriver() {
            return driver;
        }

        /**
         * The url start used for this driver.
         *
         * @return The url start used for this driver.
         */
        public String getUrl() {
            return url;
        }

        /**
         * A sample commandline.
         *
         * @return A sample commandline.
         */
        public String getExample() {
            return example;
        }

        private static List<JdbcDriverInfo> listProviders;

        private static List<JdbcDriverInfo> getDrivers() {

            if (listProviders != null)
                return listProviders;
            listProviders = new ArrayList<>();

            for (Enumeration<Driver> e = DriverManager.getDrivers(); e.hasMoreElements();) {
                Driver driver = e.nextElement();
                String className = driver.getClass().getCanonicalName();
                String lcClassName = className.toLowerCase();
                // first!!!
                if (lcClassName.contains(".fabric."))
                    listProviders.add(new JdbcDriverInfo("fabric", className, "jdbc:mysql:", "//localhost/sakila"));
                else if (lcClassName.contains(".db2."))
                    listProviders.add(new JdbcDriverInfo("db2", className, "jdbc:db2:", "//localhost:50000/SAMPLE"));
                else  if (lcClassName.contains(".intersys."))
                    listProviders.add(new JdbcDriverInfo("cache", className, "jdbc:Cache:", "//SERVER:PORT/DbName"));
                else  if (lcClassName.contains(".odbc."))
                    listProviders.add(new JdbcDriverInfo("excel", className, "jdbc:odbc:", "Driver={Microsoft Excel Driver (*.xls)};DBQ=c:\\testData.xls"));
                else  if (lcClassName.contains("oracle."))
                    listProviders.add(new JdbcDriverInfo("oracle", className, "jdbc:oracle:thin:", "@//localhost:1521/ORCL"));
                else  if (lcClassName.contains(".mysql."))
                    listProviders.add(new JdbcDriverInfo("mysql", className, "jdbc:mysql:", "//localhost/sakila"));
                else  if (lcClassName.contains(".sqlserver."))
                    listProviders.add(new JdbcDriverInfo("sql", className, "jdbc:sqlserver:", "//localhost:1433;DatabaseName=Northwind"));
                else  if (lcClassName.contains(".postgresql."))
                    listProviders.add(new JdbcDriverInfo("postgresql", className, "jdbc:postgresql:", "//localhost/pagila"));
                else {
                    // CData ones we know
                    if (lcClassName.contains("cdata.")) {
                        lcClassName = lcClassName.substring(11);
                        lcClassName = lcClassName.substring(0, lcClassName.indexOf('.'));
                        String url = "jdbc:cdata:" + lcClassName + ":";
                        listProviders.add(new JdbcDriverInfo(lcClassName, className, url, "connection_string"));
                    }
                    // now a total guess ( help at https://www.benchresources.net/jdbc-driver-list-and-url-for-all-databases/)
                    else {

                        while (lcClassName.startsWith("com.") || lcClassName.startsWith("org.") || lcClassName.startsWith("jdbc."))
                            lcClassName = lcClassName.substring(lcClassName.indexOf('.') + 1);
                        int pos = lcClassName.indexOf('.');
                        if (pos != -1)
                            lcClassName = lcClassName.substring(0, pos);
                        String url = "jdbc:" + lcClassName + ":";
                        listProviders.add(new JdbcDriverInfo(lcClassName, className, url, "connection_string"));
                    }
                }
            }

            // sort in name order
            Collections.sort(listProviders, Comparator.comparing(JdbcDriverInfo::getName));

            return listProviders;
        }
    }

    static boolean isHTMLOutput(String fileName) {
        return fileName.endsWith(".html") || fileName.endsWith(".htm") || fileName.endsWith(".xhtml");
    }

    private static ProcessReport createHTMLReport(String fileName, InputStream template, OutputStream output)
            throws SetupException {
        ProcessHtml report = new ProcessHtml(template, output);
        if (fileName.endsWith(".htm")) {
            report.setCss(ProcessHtmlAPI.CSS_NO, null, null);
        } else {
            report.setCss(ProcessHtmlAPI.CSS_INCLUDE, null, null);
            if (fileName.endsWith(".xhtml")) {
                report.getProperties().set(ReportProperties.HTML_SET_XHTML, true);
            }
        }
        if (!report.isEmbedImages()) {
            report.setImagePath(new File(fileName).getAbsoluteFile().getParent(), "", "");
        }
        return report;
    }
}
