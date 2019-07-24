/*
* Copyright (c) 2011 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

import java.io.*;
import java.lang.reflect.Method;
import java.net.*;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.*;

import net.windward.datasource.DatasetBase;
import net.windward.datasource.SelectBase;
import net.windward.datasource.SelectFilter;
import net.windward.datasource.abstract_datasource.salesforce.SalesForceDataSource;
import net.windward.datasource.json.JsonDataSource;
import net.windward.datasource.odata.ODataDataSource;
import net.windward.env.MyParsePosition;
import net.windward.env.WindwardWrapper;
import net.windward.format.htm.*;
import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.datasource.jdbc.JdbcDataSource;
import net.windward.xmlreport.*;
import org.apache.commons.codec.binary.Base64;
import org.apache.commons.io.FilenameUtils;

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

            if (!cmdLine.isPerformance())
                runOneReport(cmdLine, args.length == 2);
            else {
                new RunReport().runMultipleReports(cmdLine);
            }

            long ticks = new Date().getTime() - start.getTime();
            System.out.println("Elapsed time: " + ticksAsTime(ticks));

        } catch (Throwable t) {
            System.err.println("Error: " + t.getMessage());
            t.printStackTrace();
            throw t;
        }
    }

    private void runMultipleReports(CommandLine cmdLine) throws InterruptedException {

        File dirReports = new File(cmdLine.getReportFilename()).getAbsoluteFile().getParentFile();
        if (!dirReports.isDirectory()) {
            System.err.println("The directory " + dirReports.getAbsolutePath() + " does not exist");
            return;
        }

        // drop out threads - default is twice the number of cores.
        int numThreads = cmdLine.getNumThreads();
        numReportsRemaining = cmdLine.getNumReports();

        // run num threads
        ReportWorker[] th = new ReportWorker[numThreads];
        for (int ind = 0; ind < numThreads; ind++)
            th[ind] = new ReportWorker(ind, cmdLine);

        DateFormat df = DateFormat.getTimeInstance(DateFormat.MEDIUM);
        Date startTime = new Date();
        System.out.println("Start time: " + df.format(startTime) + ", " + numThreads + " threads, " + cmdLine.getNumReports() + " reports");
        System.out.println();
        System.out.print("[Thread number:Report number]; ");
        for (int ind = 0; ind < numThreads; ind++)
            th[ind].start();

        // we wait
        synchronized (this) {
            threadsRunning += numThreads;
            while (threadsRunning > 0)
                wait();
        }

        System.out.println();
        System.out.println();
        Date endTime = new Date();
        long elapsed = endTime.getTime() - startTime.getTime();
        System.out.println("End time: " + df.format(endTime) + ", Elapsed time: " + ticksAsTime(elapsed) + ", time per report: " + ticksAsTime(elapsed / cmdLine.getNumReports()));
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

    int numReportsRemaining;

    synchronized boolean hasNextReport() {
        numReportsRemaining--;
        return numReportsRemaining >= 0;
    }

    private int threadsRunning = 0;

    synchronized void markDone() {
        threadsRunning--;
        notify();
    }

    private class ReportWorker extends Thread {
        private int threadNum;
        private CommandLine cmdLine;

        public ReportWorker(int threadNum, CommandLine cmdLine) {
            this.threadNum = threadNum;
            this.cmdLine = cmdLine;
        }

        public void run() {

            try {
                File dirReports = new File(cmdLine.getReportFilename()).getAbsoluteFile().getParentFile();
                String extReport = cmdLine.getReportFilename().substring(cmdLine.getReportFilename().lastIndexOf('.'));
                for (int rptNum = 0; hasNextReport(); rptNum++) {
                    String reportFilename = File.createTempFile("rpt", extReport, dirReports).getAbsolutePath();
                    CommandLine cl = new CommandLine(cmdLine, reportFilename);
                    runOneReport(cl, false);
                    System.out.print("[{" + threadNum + "}:{" + (rptNum + 1) + ")}]; ");
                }
            } catch (Exception e) {
                e.printStackTrace();
            } finally {
                markDone();
            }
        }
    }

    private static void runOneReport(CommandLine cmdLine, boolean preservePodFraming) throws Exception {

        // get the template and output file streams. Output is null for printers
        InputStream template = openInputStream(cmdLine.getTemplateFilename());
        OutputStream reportOutput;

        // if it's HTML and set to split pages, no reportOutput; ProcessHtml creates page streams
        if ((cmdLine.getReportFilename().endsWith(".htm")
                || cmdLine.getReportFilename().endsWith(".html")
                || cmdLine.getReportFilename().endsWith(".xhtml"))
                && ProcessHtml.isSplitPagesFromProperties())
            reportOutput = null;
        else if (!cmdLine.getReportFilename().endsWith(".prn"))
            reportOutput = new FileOutputStream(cmdLine.getReportFilename());
        else
            reportOutput = null;
        if (!cmdLine.isPerformance()) {
            System.out.println("Template: " + cmdLine.getTemplateFilename());
            System.out.println("Report: " + cmdLine.getReportFilename());
        }

//			TemplateInfo ti = ProcessReport.getTemplateMetrics(new FileInputStream(cmdLine.getTemplateFilename()));

        // Create the report object, based on the file extension
        ProcessReportAPI report = createReport(cmdLine, template, reportOutput);

//			report.setDrillDownInfo(new ProcessReport.DrillDownTemplate("c:\\test", "abc", "def"));

        if (cmdLine.isBaseDirectorySet())
            report.setBaseDirectory(cmdLine.getBaseDirectory());

        // if we are applying no datasources then we keep the POD framing in the generated report.
        if (preservePodFraming)
            report.setPreservePodFraming(true);
        // if we have a locale, we set it (used when applying datasources).
        if (cmdLine.getLocale() != null) {
            if (!cmdLine.isPerformance())
                System.out.println("Using locale: " + cmdLine.getLocale());
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

        // This first call parses the template and prepares the report so we can apply data to it.
        report.processSetup();

        // list out vars
        if (cmdLine.getNumDatasources() > 0)
            for (Iterator it = cmdLine.getMap().entrySet().iterator(); it.hasNext(); ) {
                Map.Entry entry = (Map.Entry) it.next();
                System.out.println(entry.getKey() + " = " + entry.getValue() + " (" + entry.getValue().getClass().getName() + ")");
            }

        Map<String, DataSourceProvider> dataProviders = new HashMap<String, DataSourceProvider>();

        // Now for each datasource, we apply it to the report. This is complex because it handles all datasource types
        // as well as recording and playback.
        for (int ind = 0; ind < cmdLine.getNumDatasources(); ind++) {
            CommandLine.DatasourceInfo dsInfo = cmdLine.getDatasource(ind);


            // build the datasource
            DataSourceProvider datasource;
            InputStream dsStream = null;
            InputStream schemaStream = null;
            switch (dsInfo.getType()) {
                // An XML datasource.
                case CommandLine.DatasourceInfo.TYPE_REST:
                    if (!cmdLine.isPerformance())
                        System.out.println("XML datasource using REST: " + dsInfo.getFilename());

                    URL url = new URL(dsInfo.getFilename());
                    HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                    conn.setRequestMethod("GET");
                    conn.setRequestProperty("Accept", "application/xml");
                    conn.setRequestProperty("Content-Type", "application/xml");
                    // used to use Sun BASE64Encoder - next 2 lines
                    byte[] encodedCred = Base64.encodeBase64((dsInfo.getUsername() + ":" + dsInfo.getPassword()).getBytes());
                    String encodedCredential = new String(encodedCred, "UTF8");
                    conn.setRequestProperty("Authorization", "Basic " + encodedCredential);

                    conn.connect();

                    InputStream responseBodyStream = conn.getInputStream();
                    datasource = new Dom4jDataSource(dsStream = responseBodyStream);
                    break;

                case CommandLine.DatasourceInfo.TYPE_JSON:
                    datasource = new JsonDataSource(dsInfo.getFilename(), JsonDataSource.MODE_CONNECTION_STRING);
                    break;

                // An XML datasource.
                case CommandLine.DatasourceInfo.TYPE_XML:
                    if (!cmdLine.isPerformance()) {
                        if ((dsInfo.getSchemaFilename() == null) || (dsInfo.getSchemaFilename().length() == 0))
                            System.out.println("XML datasource: " + dsInfo.getFilename());
                        else
                            System.out.println("XML datasource: " + dsInfo.getFilename() + ", schema " + dsInfo.getSchemaFilename());
                    }

                    // Note: we have not (yet) implemented using username/password when opening local files
                    boolean authenticatorSet = false;
                    if (dsInfo.getUsername() != null && dsInfo.getPassword() != null) {
                        class URLAuthenticator extends Authenticator {
                            private String username = null;
                            private String password = null;

                            public URLAuthenticator(final String user, final String pass) {
                                super();
                                username = user;
                                password = pass;
                            }

                            protected PasswordAuthentication getPasswordAuthentication() {
                                return new PasswordAuthentication(username, password.toCharArray());
                            }
                        }
                        Authenticator.setDefault(new URLAuthenticator(dsInfo.getUsername(), dsInfo.getPassword()));
                        authenticatorSet = true;
                    }

                    try {
                        if (dsInfo.getSchemaFilename() == null)
                            datasource = new Dom4jDataSource(dsStream = openInputStream(dsInfo.getFilename()));
                        else
                            datasource = new Dom4jDataSource(dsStream = openInputStream(dsInfo.getFilename()), schemaStream = openInputStream(dsInfo.getSchemaFilename()));
                    } finally {
                        if (authenticatorSet)
                            Authenticator.setDefault(null);
                    }
                    break;

                // An OData datasource.
                case CommandLine.DatasourceInfo.TYPE_ODATA:
                    if (!cmdLine.isPerformance())
                        System.out.println("OData datasource: " + dsInfo.getFilename());

                    if (dsInfo.getUsername() != null && dsInfo.getPassword() != null)
                        datasource = new ODataDataSource(dsInfo.getFilename(), "BASIC", dsInfo.getUsername(), dsInfo.getPassword());
                    else
                        datasource = new ODataDataSource(dsInfo.getFilename());

                    break;

                //A SalesForce datsource.
                case CommandLine.DatasourceInfo.TYPE_SFORCE:
                    if (!cmdLine.isPerformance())
                        System.out.println("SalesForce datasource: " + dsInfo.getFilename());
                    datasource = new SalesForceDataSource(dsInfo.username, dsInfo.password, "", true); //security token field is empty string because expected password input is password+security token
                    break;

                case CommandLine.DatasourceInfo.TYPE_SQL:
                    if (!cmdLine.isPerformance())
                        System.out.println(dsInfo.getSqlDriverInfo().getName() + " datasource: " + dsInfo.getConnectionString());
                    datasource = new JdbcDataSource(dsInfo.getSqlDriverInfo().getDriver(),
                            dsInfo.getSqlDriverInfo().getUrl() + dsInfo.getConnectionString(),
                            dsInfo.getUsername(), dsInfo.getPassword());
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

       report.setParameters(cmdLine.getMap());
       report.processData(dataProviders);

        if (!cmdLine.isPerformance()) {
            System.out.println("All data applied, generating final report...");
        }

        // Now that all the data has been applied, we generate the final output report. This does the
        // page layout and then writes out the output file.
        report.processComplete();

        // If it is an html report, has images, and embedded images option not chosen, we write these out
        if (report instanceof ProcessHtml && !((ProcessHtml) report).isEmbedImages()) {
            ArrayList images = ((ProcessHtmlAPI) report).getImageNames();
            String dir = cmdLine.getReportFilename();
            if (dir.lastIndexOf(File.separatorChar) != -1)
                dir = dir.substring(0, dir.lastIndexOf(File.separatorChar));
            for (int ind = 0; ind < images.size(); ind++) {
                HtmlImage img = (HtmlImage) images.get(ind);
                String filename = new File(dir, img.getName()).getAbsolutePath();
                if (!cmdLine.isPerformance())
                    System.out.println("Save image: " + filename);
                FileOutputStream file = new FileOutputStream(filename);
                ((ByteArrayOutputStream) img.getStream()).writeTo(file);
                file.close();
            }
        }

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

        if (!cmdLine.isPerformance())
            System.out.println("Report complete, " + report.getNumPages() + " pages long");
        report.close();

        if (cmdLine.isLaunch()) {
            String filename = cmdLine.getReportFilename();
            System.out.println("launching report " + filename);

            try {
                // com.google.code.appengine.awt.Desktop.getDesktop().open(new File(filename));
                Class classDesktop = Class.forName("com.google.code.appengine.awt.Desktop");
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
    }

    private static ProcessReportAPI createReport(CommandLine cmdLine, InputStream template, OutputStream reportOutput) throws SetupException {

        ProcessReportAPI report;
        if (cmdLine.getReportFilename().endsWith(".htm")) {
            // base html report
            report = new ProcessHtml(template, reportOutput);
            ((ProcessHtmlAPI) report).setCss(ProcessHtmlAPI.CSS_NO, null, null);
            return report;
        }
        if (cmdLine.getReportFilename().endsWith(".html")) {
            // html 4.01 with css report
            report = new ProcessHtml(template, reportOutput);
            ((ProcessHtmlAPI) report).setCss(ProcessHtmlAPI.CSS_INCLUDE, null, null);
            return report;
        }
        if (cmdLine.getReportFilename().endsWith(".pdf")) {
            report = new ProcessPdf(template, reportOutput);
            return report;
        }
        if (cmdLine.getReportFilename().endsWith(".rtf"))
            return new ProcessRtf(template, reportOutput);
        if (cmdLine.getReportFilename().endsWith(".txt"))
            return new ProcessTxt(template, reportOutput);
        else if (cmdLine.getReportFilename().endsWith(".xhtml")) {
            // xhtml
            report = new ProcessHtml(template, reportOutput);
            ((ProcessHtmlAPI) report).setCss(ProcessHtmlAPI.CSS_INCLUDE, null, null);
            ((ProcessHtmlAPI) report).setSpec(ProcessHtmlAPI.XHTML);
            return report;
        }
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
        throw new IllegalArgumentException("output file must end with docx, docm, htm, html, pdf, pptx, pptm, rtf, sml, txt, xhtml, xls, xlsx, xlsm, or xml");
    }

    private static void DisplayUsage() {
        System.out.println("Windward Reports version " + ProcessReport.getVersion());
        System.out.println("usage: RunReport template_file output_file [-basedir path] [-xml xml_file | -sql connection_string | -oracle connection_string | -ole oledb_connection_string] [licenseKey=value | ...]");
        System.out.println("       The template file can be a rtf, xml (WordML), docx, pptx, or xlsx file.");
        System.out.println("       The output file extension determines the report type created:");
        System.out.println("           output.csv - SpreadSheet CSV file");
        System.out.println("           output.docx - Word 2007+ DOCX file");
        System.out.println("           output.htm - HTML file with no CSS");
        System.out.println("           output.html - HTML file with CSS");
        System.out.println("           output.pdf - Acrobat PDF file");
        System.out.println("           output.pptx - PowerPoint 2007+ PPTX file");
        System.out.println("           output.rtf - Rich Text Format file");
        System.out.println("           output.sml - Excel 2003+ SpreadsheetML file (rename to .xml to use)");
        System.out.println("           output.txt - Ascii text file");
        System.out.println("           output.xhtml - XHTML file with CSS");
        System.out.println("           output.xml - WordML 2003+ XML file");
        System.out.println("           output.xls - Excel XLS file");
        System.out.println("           output.xlsx - Excel 2007+ XLSX file");
        System.out.println("           output.xlsm - Excel 2007+ macro enabled XLSM file");
        System.out.println("       -performance:123 - will run the report 123 times.");
        System.out.println("            output file is used for directory and extension for reports");
        System.out.println("       -threads:4 - will create 4 threads when running -performance.");
        System.out.println("       -launch - will launch the report when complete.");
        System.out.println("       -data filename.cml - will write data.xml to this filename.");
        System.out.println("       -embed - will embed data.xml in the generated report. DOCX, PDF, PPTX, & XLSX only.");
        System.out.println("       version=9 - sets the template to the passed version (9 in this example)");
        System.out.println("       -record filename - records the next datasource to this file");
        System.out.println("       The datasource is identified with a pair of parameters");
        System.out.println("           -xml filename - passes an xml file as the datasource");
        System.out.println("                -xml xmlFilename;schemaFilename - passes an xml file and a schema file as the datasource");
        for (int ind = 0; ind < JdbcDriverInfo.drivers.length; ind++)
            System.out.println("           -" + JdbcDriverInfo.drivers[ind].getName() + " connection_string - ex: " + JdbcDriverInfo.drivers[ind].getExample());
        System.out.println("           -rest filename - passes an xml file as the datasource reading it with the REST protocol");
        System.out.println("           -odata url - passes a url as the datasource accessing it using the OData protocol");
        System.out.println("           -sforce - password should be password+securitytoken");
        System.out.println("           -[xml|sql|...]:name names this datasource with name");
        System.out.println("                set username=user password=pass BEFORE datasource for database connections");
        System.out.println("                for a POD file (datasets), set pod=pod_filename");
        System.out.println("                     must come BEFORE each -xml, -sql, ... part");
        System.out.println("            -playback filename - passes an recorded file as the datasource");
        System.out.println("       You can have 0-N key=value pairs that are passed to the datasource Map property");
        System.out.println("            If the value starts with I', F', or D' it parses it as an integer, float, or date(yyyy-MM-ddThh:mm:ss)");
        System.out.println("                example  date=\"D'1996-08-29\"");
        System.out.println("            If the value is * it will set a filter of all");
        System.out.println("            If the value is \\\"text,text,...\\\" it will set a filter of all");
    }

    private static InputStream openInputStream(String filename) throws IOException {

        int pos = filename.indexOf(':');
        if ((pos != -1) && (pos != 1))
            return new URL(filename).openStream();
        return new FileInputStream(filename);
    }

    /**
     * This class contains everything passed in the command line. It makes no calls to Windward Reports.
     */
    private static class CommandLine {
        private String templateFilename;
        private String reportFilename;
        private Map<String,Object> map;
        private List datasources;
        private Locale locale;
        private boolean launch;
        private int templateVersion;
        private int numReports;
        private int writeTags;
        private int numThreads;
        private int dataMode;
        private String dataFileName;
        private String baseDirectory;

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
            map = new HashMap<String,Object>();
            datasources = new ArrayList();
            writeTags = -1;
            numThreads = Runtime.getRuntime().availableProcessors() * 2;
        }

        private static String GetFullPath(String filename) {
            int pos = filename.indexOf(':');
            if ((pos == -1) || (pos == 1))
                return new File(filename).getAbsolutePath();
            return filename;
        }

        public CommandLine(CommandLine src, String report) {
            templateFilename = src.templateFilename;
            map = src.map;
            datasources = src.datasources;
            locale = src.locale;
            launch = src.launch;
            templateVersion = src.templateVersion;
            writeTags = src.writeTags;
            numReports = src.numReports;
            numThreads = src.numThreads;
            dataMode = src.dataMode;
            dataFileName = src.dataFileName;
            baseDirectory = src.baseDirectory;

            reportFilename = report;
        }

        /**
         * The name of the template file.
         *
         * @return The name of the template file.
         */
        public String getTemplateFilename() {
            return templateFilename;
        }

        /**
         * The name of the report file. null for printer reports.
         *
         * @return The name of the report file. null for printer reports.
         */
        public String getReportFilename() {
            return reportFilename;
        }

        /**
         * The parameters passed for each datasource to be created.
         *
         * @return The parameters passed for each datasource to be created.
         */
        public List getDatasources() {
            return datasources;
        }

        /**
         * The parameters passed for each datasource to be created.
         *
         * @param datasources The parameters passed for each datasource to be created.
         */
        public void setDatasources(List datasources) {
            this.datasources = datasources;
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
            return (DatasourceInfo) datasources.get(index);
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

        /**
         * The parameters passed for a single datasource. All filenames are expanded to full paths so that if an exception is
         * thrown you know exactly where the file is.
         */
        private static class DatasourceInfo {

            /**
             * An XML file using the REST protocol.
             */
            public static final int TYPE_REST = 0;

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

            private int type;
            private String name;

            private String filename;
            private String schemaFilename;

            private JdbcDriverInfo sqlDriverInfo;
            private String connectionString;

            private String username;
            private String password;
            private String podFilename;

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
                return schemaFilename;
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
        }

        /**
         * Create a CommandLine object from the command line passed to the program.
         *
         * @param args The arguments passed to the program.
         * @return A CommandLine object populated from the args.
         */
        public static CommandLine Factory(String[] args) {

            CommandLine rtn = new CommandLine(args[0], args[1]);

            String username = null, password = null, podFilename = null;

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

                if (cmd.equals("-launch")) {
                    rtn.setLaunch(true);
                    continue;
                }

                if (cmd.equals("-basedir")) {
                    rtn.baseDirectory = args[++ind];
                    continue;
                }

                if (cmd.equals("-xml") || cmd.equals("-rest")) {
                    String xmlFilename = args[++ind];
                    int split = xmlFilename.indexOf(';');
                    String schemaFilename;
                    if (split == -1)
                        schemaFilename = null;
                    else {
                        schemaFilename = xmlFilename.substring(split + 1).trim();
                        xmlFilename = xmlFilename.substring(0, split).trim();
                    }
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, xmlFilename, schemaFilename, username, password, podFilename,
                            cmd.equals("-rest") ? DatasourceInfo.TYPE_REST : DatasourceInfo.TYPE_XML);
                    rtn.datasources.add(datasourceOn);
                    username = password = podFilename = null;
                    continue;
                }

                if (cmd.equals("-json")) {
                    String url = args[++ind];
                    DatasourceInfo datasourceOn = new DatasourceInfo(name, url, username, password, podFilename, DatasourceInfo.TYPE_JSON);
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
                boolean isDb = false;
                for (int index = 0; index < JdbcDriverInfo.drivers.length; index++) {
                    JdbcDriverInfo di = JdbcDriverInfo.drivers[index];
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
                if (equ == -1)
                    throw new IllegalArgumentException("Unknown option " + args[ind]);
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
                if (key.equals("writeTags")) {
                    value = value.toLowerCase().trim();
                    if (value.equals("text"))
                        rtn.setWriteTags(ProcessReportAPI.TAG_STYLE_TEXT);
                    else if (value.equals("fields"))
                        rtn.setWriteTags(ProcessReport.TAG_STYLE_FIELD);
                    else if (value.equals("fields2007"))
                        rtn.setWriteTags(ProcessReport.TAG_STYLE_FIELD_2007);
                    else if (value.equals("controls"))
                        rtn.setWriteTags(ProcessReport.TAG_STYLE_CONTROL_2007);
                    else if (value.equals("linkPptx"))
                        rtn.setWriteTags(ProcessReport.TAG_STYLE_LINK_PPTX);
                    else
                        System.err.println("Warning: unrecognized value for writeTags key. Valid values include: text, fields, fields2007, controls.");
                }

                Object val;
                // may be a list
                if (value.indexOf(',') != -1) {
                    val = new ArrayList();
                    StringTokenizer tok = new StringTokenizer(value, ",", false);
                    while (tok.hasMoreTokens()) {
                        String elem = tok.nextToken();
                        ((ArrayList) val).add(convertValue(elem));
                    }
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
                MyParsePosition pp = new MyParsePosition(0);
                SimpleDateFormat stdFmt = new SimpleDateFormat("yyyy-MM-dd'T'hh:mm:ss");
                Date date = stdFmt.parse(value.substring(2), pp);
                if ((date != null) && (pp.getIndex() > 0))
                    return date;
                stdFmt = new SimpleDateFormat("yyyy-MM-dd");
                date = stdFmt.parse(value.substring(2), pp);
                if ((date != null) && (pp.getIndex() > 0))
                    return date;
                throw new IllegalArgumentException("Could not parse yyyy-MM-dd[Thh:mm:ss] date from " + value.substring(2));
            }
            return value;
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

        private static JdbcDriverInfo[] drivers = {
                new JdbcDriverInfo("cache", "com.intersys.jdbc.CacheDriver", "jdbc:Cache:", "//SERVER:PORT/DbName"),
                new JdbcDriverInfo("db2", "com.ibm.db2.jcc.DB2Driver", "jdbc:db2:", "//localhost:50000/SAMPLE"),
                new JdbcDriverInfo("excel", "sun.jdbc.odbc.JdbcOdbcDriver", "jdbc:odbc:", "Driver={Microsoft Excel Driver (*.xls)};DBQ=c:\\testData.xls"),
                new JdbcDriverInfo("oracle", "oracle.jdbc.driver.OracleDriver", "jdbc:oracle:thin:", "@//localhost:1521/ORCL"),
                new JdbcDriverInfo("mysql", "com.mysql.jdbc.Driver", "jdbc:mysql:", "//localhost/sakila"),
                new JdbcDriverInfo("sql", "com.microsoft.sqlserver.jdbc.SQLServerDriver", "jdbc:sqlserver:", "//localhost:1433;DatabaseName=Northwind"),
                new JdbcDriverInfo("postgresql", "org.postgresql.Driver", "jdbc:postgresql:", "//localhost/pagila"),
        };
    }
}
