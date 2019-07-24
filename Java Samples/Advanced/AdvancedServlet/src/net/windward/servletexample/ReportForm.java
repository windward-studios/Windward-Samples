/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package net.windward.servletexample;

import javax.servlet.ServletContext;
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

public class ReportForm extends HttpServlet {


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
	    out.println("<title>Windward Engine Demonstration</title>");
		out.println("<style type=text/css> div { text-align:center; } tr { text-align:left; } table { margin-left:auto;margin-right:auto; } .floatRight { float:right } </style>");
        out.println("</head>");
		
        out.println("<body>");
		
		out.println("<p class=\"floatRight\"> Java Engine Demo </p>");
		out.println("<img src=\"./files/Windward.png\" width=\"260\" height=\"43\" alt=\"Run a sample report\"/>");
		out.println("<div>");
		out.println("<h3>Windward Personnel Leave of Absence</h3>");
		
		// form that asks for the report template, the data set, and the output format
		out.println( "<form action=\"notice\" method=post name=\"report2\">" );
		out.println("<p>Leave Request</p>");
		
		// we get our templates & data files from the context
		ServletContext context = getServletContext();
		// we attach matching files to the session
		HttpSession session = request.getSession();
		
		out.println("<p>");
		out.println("<select name=\"var\">");
		session.setAttribute("1 - Maria Anders, 5/1/2009", "1");
		session.setAttribute("2 - Frederique Citeaux, 4/12/2009", "2");
		session.setAttribute("3 - Maria Anders, 6/29/2009", "3");
		session.setAttribute("4 - Laurence Lebihan, 6/1/2009", "4");
		session.setAttribute("5 - Christina Berglund, 8/12/2009", "5");
		session.setAttribute("6 - Thomas Hardy, 7/1/2009", "6");
		out.println("<option>" + "1 - Maria Anders, 5/1/2009" + "</option>");
		out.println("<option>" + "2 - Frederique Citeaux, 4/12/2009" + "</option>");
		out.println("<option>" + "3 - Maria Anders, 6/29/2009" + "</option>");
		out.println("<option>" + "4 - Laurence Lebihan, 6/1/2009" + "</option>");
		out.println("<option>" + "5 - Christina Berglund, 8/12/2009" + "</option>");
		out.println("<option>" + "6 - Thomas Hardy, 7/1/2009" + "</option>");
		out.println("</select>");
		out.println("</p>");
		
		out.println("<table>");
		out.println("<tr> <td> <input type=\"radio\" checked name=\"format\" value=\"" + GenerateReport.Format.PDF + "\">Adobe PDF</input> </td> </tr>");
		out.println("<tr> <td> <input type=\"radio\" name=\"format\" value=\"" + GenerateReport.Format.DOCX + "\">Microsoft Word</input> </td> </tr>");
		out.println("<tr> <td> <input type=\"radio\" name=\"format\" value=\"" + GenerateReport.Format.HTML + "\">HTML</input> </td> </tr>");
		out.println("</table>");
		
		out.println("<p> <button type=\"submit\" value=\"Generate Report\">Create Letter</button> </p>");
		
		out.println("</form>");
	}
}
