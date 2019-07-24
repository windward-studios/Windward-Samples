/*
* Copyright (c) 2010 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package com.windwardreports;

import java.io.*;
import java.text.*;
import java.util.*;
import javax.servlet.*;
import javax.servlet.http.*;

import net.windward.xmlreport.*;

/**
 * This class creates a form for selecting a report to generate.
 *
 * @author En-jay Hsu
 * @version 1.1  October 22, 2010
 */

public class BasicReportForm extends HttpServlet {


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

        // header boilerplate
        response.setContentType("text/html");
        PrintWriter out = response.getWriter();

        out.println("<html>");

        out.println("<head>");
        out.println("<title>Windward Engine Demonstration</title>");
        out.println("<style type=text/css> td { width:200px; } .floatRight { float:right;text-align:right; } </style>");
        out.println("</head>");

        out.println("<body>");

        out.println("<p class=\"floatRight\"> Java Engine Demo </p>");

        // form that asks for the report template, the data set, and the output format
        out.println("<form action=\"notice\" method=post name=\"report\">");
        // we attach session data to the session
        HttpSession session = request.getSession();
        out.println("<h3>This example will output a PDF using a DB2 datasource<h3>");
        session.setAttribute("var", "1");
        out.println("<p> <button type=\"submit\" value=\"Generate Report\">Create Report</button> </p>");

        out.println("</form>");
    }
}
