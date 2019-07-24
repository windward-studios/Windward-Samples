/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package net.windward.servletexample;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;
import java.io.IOException;
import java.io.PrintWriter;

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
	 * @param request An HttpServletRequest object that contains the request the client has made 
	 * of the servlet.
	 *
	 * @param response An HttpServletResponse object that contains the response the servlet sends 
	 * to the client.
	 *
	 * @exception IOException If an input or output error is detected when the servlet handles the request.
	 *
	 * @exception ServletException If the request for the POST could not be handled.
	 */
    public void doPost(HttpServletRequest request, HttpServletResponse response) throws IOException, ServletException {
	
		doGet( request, response );
	}
		
	/**
	 * Called by the server (via the service method) to allow a servlet to handle a GET request.  
	 *
	 * @param request An HttpServletRequest object that contains the request the client has made 
	 * of the servlet.
	 *
	 * @param response An HttpServletResponse object that contains the response the servlet sends 
	 * to the client.
	 *
	 * @exception IOException If an input or output error is detected when the servlet handles the request.
	 *
	 * @exception ServletException If the request for the POST could not be handled.
	 */
    public void doGet(HttpServletRequest request, HttpServletResponse response) throws IOException, ServletException {

		// header boilerplate
        response.setContentType("text/html");
        PrintWriter out = response.getWriter();

        out.println("<html>");
		
        out.println("<head>");
	    out.println("<title>Windward for Google App Engine Demonstration</title>");
		out.println("<style type=text/css> td { width:200px; } .floatRight { float:right;text-align:right; } </style>");
        out.println("</head>");
		
        out.println("<body>");
		
		out.println("<p class=\"floatRight\"> Java Engine Demo </p>");
		out.println("<img src=\"./files/Windward.png\" width=\"260\" height=\"43\" alt=\"Run a sample report\"/>");
		out.println("<div align=\"center\">");
		out.println("<h3>Windward Personnel Leave of Absence</h3>");
		
		// form that asks for the report template, the data set, and the output format
		out.println( "<form action=\"notice\" method=post name=\"report\">" );
		// we attach session data to the session
		HttpSession session = request.getSession();
		session.setAttribute("var", "1");
		session.setAttribute("format", GenerateReport.Format.PDF);
		
		out.println("<table border=\"1\">");
		out.println("<tr align=\"left\"> <td>Leave Request Id:</td> <td>#1</td> </tr>");
		out.println("<tr align=\"left\"> <td>Employee Name:</td> <td>Maria Anders</td> </tr>");
		out.println("<tr align=\"left\"> <td>Employee ID:</td> <td>12209</td> </tr>");
		out.println("<tr align=\"left\"> <td>Manager Name:</td> <td>Hanna Moos</td> </tr>");
		out.println("<tr align=\"left\"> <td>Manager Email:</td> <td>hannam@ng.com</td> </tr>");
		out.println("<tr align=\"left\"> <td>Leave Start Date:</td> <td>5/12/2009</td> </tr>");
		out.println("<tr align=\"left\"> <td>Leave End Date:</td> <td>5/27/2009</td> </tr>");
		out.println("</table>");
		
		out.println( "<p> <button type=\"submit\" value=\"Generate Report\">Create Letter</button> </p>" );
		
		out.println( "</form>" );
	}
}
