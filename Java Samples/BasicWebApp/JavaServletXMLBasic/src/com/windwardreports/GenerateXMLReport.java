/*
* Copyright (c) 2012 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package com.windwardreports;
import java.io.*;
import java.lang.String;
import javax.servlet.*;
import javax.servlet.http.*;
import net.windward.datasource.*;
import net.windward.xmlreport.*;
import net.windward.datasource.dom4j.Dom4jDataSource;

/**
 * This class generates the report and returns it to the browser.
 *
 * @author En-jay Hsu
 * @version 1.1  October 22, 2010
 */

public class GenerateXMLReport extends HttpServlet {

    private String propFile = ".";
    /**
     * Called by the servlet container to indicate to a servlet that the servlet is being
     * placed into service. Set the location of WindwardReports.properties here.
     */
    public void init() {
        ServletContext context = getServletContext();
        propFile = context.getInitParameter("PropFile");
        if (propFile == null)
            propFile = "WindwardReports.properties";
        propFile = context.getRealPath(propFile);
        System.setProperty("WindwardReports.properties.filename", propFile);
        log("Windward Reports property file at: " + propFile);
    }

    /**
     * Called by the server (via the service method) to allow a servlet to handle a POST request.
     * The HTTP POST method allows the client to send data of unlimited length to the Web server
     * a single time and is useful when posting information such as credit card numbers.
     *
     * @param request  An HttpServletRequest object that contains the request the client has made
     *                 of the servlet.
     * @param response An HttpServletResponse object that contains the response the servlet sends
     *                 to the client.
     * @throws IOException      If an input or output error is detected when the servlet handles the request.
     * @throws ServletException If the request for the POST could not be handled.
     */
    public void doPost(HttpServletRequest request, HttpServletResponse response) throws IOException, ServletException {

        doGet(request, response);
    }

    /**
     * Called by the server (via the service method) to allow a servlet to handle a GET request.
     *
     * @param request  An HttpServletRequest object that contains the request the client has made
     *                 of the servlet.
     * @param response An HttpServletResponse object that contains the response the servlet sends
     *                 to the client.
     * @throws IOException      If an input or output error is detected when the servlet handles the request.
     * @throws ServletException If the request for the POST could not be handled.
     */
    public void doGet(HttpServletRequest request, HttpServletResponse response) throws IOException, ServletException {

        ServletContext context = getServletContext();
        HttpSession session = request.getSession();
        String template = "/files/template.docx";
        String data = "/files/data.xml";

        // make sure we have a license key
        try {
            ProcessReport.init();
        } catch (Exception le) {
            log("License error", le);
            throw new ServletException("License error", le);
        }

        if (session.getAttribute("var") == null) {
            response.sendRedirect("http://localhost:8080/windward/report");
            return;
        }

        // get an input stream to the template & xml
        InputStream templateFile = context.getResourceAsStream(template);
        InputStream xmlFile = context.getResourceAsStream(data);
        ByteArrayOutputStream reportStream = new ByteArrayOutputStream();

        // if can't find the files - we're done
        if ((templateFile == null)) {
            FileNotFoundException fnfe = new FileNotFoundException("Could not find file: " + context.getRealPath(template));
            throw new ServletException("Could not open template and/or data file", fnfe);
        }

        // create the report
        ProcessReport report;
        try {
            report = new ProcessPdf(templateFile, reportStream);
            report.processSetup();
            DataSourceProvider dsp= new Dom4jDataSource(xmlFile);
            report.processData(dsp, "Orders");
            dsp.close();
        } catch (Exception se) {
            log("Setup error", se);
            throw new ServletException("Setup error", se);
        }

        try {
            report.processComplete();
        } catch (Exception e) {
            log("process threw exception", e);
            throw new ServletException("ProcessReport.process() threw exception", e);
        }

        templateFile.close();
        xmlFile.close();

        //display the pdf page
        response.setContentType("application/pdf");

        // IE really really wants this for pdf files
        response.setBufferSize(reportStream.size());

        ServletOutputStream out = response.getOutputStream();
        reportStream.writeTo(out);

        reportStream.close();
    }
}



