/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package net.windward.servletexample;

import javax.servlet.ServletContext;
import javax.servlet.ServletException;
import javax.servlet.ServletOutputStream;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;

/**
 * This class only contains a get method to run all templates or a specific
 * template through the engine. The response is the report if a single template
 * is specified, or a list of the templates, and for each templates, any errors
 * that occured while processing.
 *
 * @author marcusj
 */
public class RunAllTemplates extends HttpServlet {
    // list of templates to run if no specific template is specified
    Collection<Template> templatesToRun;

    // map of templates; specific template run by setting test parameter to template's key
    Map<String, Template> templatesMap;

    @Override
    public void init() throws ServletException {
        super.init();

        // Populate the templates array
        populateTemplates();

        // Get the servlet context; this persists across requests
        ServletContext context = getServletContext();

        // Set properties file -- This is a context-param in web.xml.
        GenerateReport.propFile = context.getInitParameter("PropFile");
        if (GenerateReport.propFile == null)
            GenerateReport.propFile = "/WEB-INF/WindwardReports.properties";
        GenerateReport.propFile = context.getRealPath(GenerateReport.propFile);
        System.setProperty("WindwardReports.properties.filename", GenerateReport.propFile);
        log("Windward Reports property file at: " + GenerateReport.propFile);
    }

    @Override
    public void destroy() {
        super.destroy();
    }

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
        // Get response writer
        resp.setContentType("text/html");

        // Get template
        String testToRun = req.getParameter("test");

        // no test parameter? do them all
        if (testToRun == null) {
            runAllTemplates(resp);
            return;
        }
        // test parameter but no matching template? prompt
        if (!templatesMap.keySet().contains(testToRun)) {
            // not running template now, so use output to display message.
            PrintWriter out = resp.getWriter();

            openHTMLResponse(out, "Invalid parameter");
            out.println("<p>Invalid value for parameter test.  Valid options are:</p>");
            out.println("<ul>");
            String runallPath = getServletContext().getContextPath() + "/runall";
            for (String key : templatesMap.keySet()) {
                out.println("<li><a href=\"" + runallPath + "?test=" + key + "\">" + key + "</a></li>");
            }
            out.println("</ul>");
            out.println("<p>Or if you prefer, you can <a href=\"" + runallPath + "\">run them all</a></p>");
            closeHTMLResponse(out);
            return;
        }

        // process template requested
        Template template = templatesMap.get(testToRun);
        ByteArrayOutputStream reportStream = new ByteArrayOutputStream();
        GenerateReport.RunReport(getServletContext(), template, null, reportStream);

        // put the template in the response
        switch (template.format) {
            case PDF:
                // note - this sometimes doesn't work with IE. It works fine with Netscape & Opera
                resp.setContentType("application/pdf");
                break;
            case RTF:
                resp.setContentType("application/rtf");
            case DOCX:
                resp.setContentType("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                break;
            case XLSX:
                resp.setContentType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            case HTML:
                // It should not be text/css for the html with css
                resp.setContentType("text/html");
                break;
            default:
                resp.setContentType("text/plain");
        }
        ServletOutputStream out = resp.getOutputStream();
        reportStream.writeTo(out);
        reportStream.close();
    }

    private void runAllTemplates(HttpServletResponse resp) throws IOException {
        PrintWriter out = resp.getWriter();

        // beginning HTML content
        openHTMLResponse(out, "Run All Templates");

        for (Template template : templatesToRun) {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            doTemplateHTMLResponse(out, template, baos);
        }

        // close HTML page
        closeHTMLResponse(out);
    }

    private void doTemplateHTMLResponse(PrintWriter out, Template template, ByteArrayOutputStream reportStream) {
        // run a template and print out any errors in HTML
        getServletContext().log("Processing template: " + template.template);
        out.println("<h1>" + template.template + "</h1>");
        out.println("<h2>Details:</h2>");
        out.println("<p>");
        out.println("Format: " + template.format.toString() + "<br />");
        out.println("Datasource type: " + template.datasourceType.toString() + "</p>");
        try {
            GenerateReport.RunReport(getServletContext(), template, null, reportStream);
        } catch (Throwable t) {
            getServletContext().log("*********************ERROR**********************");
            out.println("<h2>Errors:</h2>");
            out.println("<p>");
            out.println(t.getMessage());
            out.println("</p>");
            out.println("<p>");
            out.println(displayErrorForWeb(t));
            out.println("</p>");
        }
        out.println("<hr />");
    }

    private void openHTMLResponse(PrintWriter out, String title) {
        // output what goes at the beginning of an HTML document (before body)
        getServletContext().log("Opening HTML Response");
        out.println("<html>");
        out.println(String.format("<head><title>ServletSample - %s</title></head>", title));
        out.println("<body>");
    }

    private void closeHTMLResponse(PrintWriter out) {
        // output what goes at the end of an HTML document (after body).
        getServletContext().log("Closing HTML Response");
        out.println("</body>");
        out.println("</html>");
    }

    private String displayErrorForWeb(Throwable t) {
        // reprint error message with HTML line breaks
        StringWriter sw = new StringWriter();
        PrintWriter pw = new PrintWriter(sw);
        t.printStackTrace(pw);
        String stackTrace = sw.toString();
        return stackTrace
                .replace(System.getProperty("line.separator"), "<br/>\n")
                .replace("Caused by: ", "<h3>Caused by:</h3>");
    }

    private void populateTemplates() {
        templatesMap = new HashMap<String, Template>();
        templatesMap.put("DOCX", new FileDatasourceTemplate("/samples/templates/Smart Energy Template.docx", GenerateReport.DatasourceType.XML, GenerateReport.Format.DOCX, "/samples/data/Smart Energy.xml"));
        templatesMap.put("PDF", new FileDatasourceTemplate("/samples/templates/Smart Energy Template.docx", GenerateReport.DatasourceType.XML, GenerateReport.Format.PDF, "/samples/data/Smart Energy.xml"));
        templatesMap.put("HTML", new FileDatasourceTemplate("/samples/templates/Smart Energy Template.docx", GenerateReport.DatasourceType.XML, GenerateReport.Format.HTML, "/samples/data/Smart Energy.xml"));
        templatesMap.put("RTF", new FileDatasourceTemplate("/samples/templates/Smart Energy Template.docx", GenerateReport.DatasourceType.XML, GenerateReport.Format.RTF, "/samples/data/Smart Energy.xml"));
        templatesMap.put("XLSX", new FileDatasourceTemplate("/samples/templates/WesternSlope Template.xlsx", GenerateReport.DatasourceType.XML, GenerateReport.Format.XLSX, "/samples/data/WesternSlope Data.xml"));
        // Don't test XML because it's covered above
        templatesMap.put("DB2", new URLDatasourceTemplate("/samples/templates/DB2 - Template.docx", GenerateReport.DatasourceType.DB2, GenerateReport.Format.PDF, "demo", "demo", "db2.windward.net", "SAMPLE"));
        templatesMap.put("Excel", new FileDatasourceTemplate("/samples/templates/Microsoft Excel File Datasource - Template.docx", GenerateReport.DatasourceType.Excel, GenerateReport.Format.PDF, "/samples/data/Northwind Mini - Data.xlsx"));
        templatesMap.put("Access", new FileDatasourceTemplate("/samples/templates/Microsoft Access Datasource Connection - Template.docx", GenerateReport.DatasourceType.Access, GenerateReport.Format.PDF, "/samples/data/Northwind - 2007.accdb"));
        templatesMap.put("MSSQL", new URLDatasourceTemplate("/samples/templates/Microsoft SQL Server - Template.docx", GenerateReport.DatasourceType.SqlServer, GenerateReport.Format.PDF, "demo", "demo", "mssql.windward.net", "Northwind"));
        templatesMap.put("MySQL", new URLDatasourceTemplate("/samples/templates/MySQL - Template.docx", GenerateReport.DatasourceType.MySQL, GenerateReport.Format.PDF, "test", "test", "mysql.windward.net", "sakila"));
        templatesMap.put("Oracle", new URLDatasourceTemplate("/samples/templates/Oracle - Template.docx", GenerateReport.DatasourceType.Oracle, GenerateReport.Format.PDF, "hr", "hr", "oracle.windward.net:1521", null));
        templatesMap.put("OData", new URLDatasourceTemplate("/samples/templates/OData - Template.docx", GenerateReport.DatasourceType.OData, GenerateReport.Format.PDF, null, null, "http://odata.windward.net/Northwind/Northwind.svc/", null));
        templatesMap.put("Salesforce", new SalesforceTemplate("/samples/templates/Salesforce - Template.docx", GenerateReport.DatasourceType.Salesforce, GenerateReport.Format.PDF, "sfdcdev@windward.net", "Puppy5Donut!", "jWAnvtBi1xCMIjXpv0aurE91"));
        templatesMap.put("JSON", new FileDatasourceTemplate("/samples/templates/JSON - Template.docx", GenerateReport.DatasourceType.JSON, GenerateReport.Format.PDF, "http://json.windward.net/Northwind.json"));
        templatesMap.put("SmartArt", new FileDatasourceTemplate("/samples/templates/Shapes and SmartArt - Template.docx", GenerateReport.DatasourceType.NONE, GenerateReport.Format.DOCX, null));
        templatesMap.put("SmartArtPDF", new FileDatasourceTemplate("/samples/templates/Shapes and SmartArt - Template.docx", GenerateReport.DatasourceType.NONE, GenerateReport.Format.PDF, null));

        templatesToRun = new ArrayList<Template>();
        templatesToRun.addAll(templatesMap.values());

        // Remove some that seem to cause infinite loops (the Smart Energy ones
        // are most likely fixed by the time you see this comment)
        templatesToRun.remove(templatesMap.get("DOCX"));
        templatesToRun.remove(templatesMap.get("PDF"));
        templatesToRun.remove(templatesMap.get("HTML"));
        templatesToRun.remove(templatesMap.get("RTF"));
    }
}