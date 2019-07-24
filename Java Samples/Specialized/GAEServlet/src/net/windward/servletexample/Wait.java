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
import java.util.Calendar;
import java.util.Enumeration;

/**
 * This class puts up a wait screen and then redirects to the report generator.
 *
 * @author David Thielen
 * @version 1.0  March 20, 2003
 */

public class Wait extends HttpServlet {


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
	
		// we need to attach the form posts to the session because a refresh does a get
		HttpSession session = request.getSession();
		Enumeration en = request.getParameterNames();
		while (en.hasMoreElements()) {
			String name = (String) en.nextElement();
			String [] values = request.getParameterValues( name );
			for (String value : values) session.setAttribute(name, value);
		}

		// A simple wait screen that will forward to the report generator.
        response.setContentType("text/html");
        PrintWriter out = response.getWriter();

        out.println("<html>");
        out.println("<head>");

	    out.println( "<title>Windward for Google App Engine Demo - Processing</title>" );
		out.println( "<meta http-equiv=\"refresh\" content=\"0; url=generate\">" );
        out.println( "</head>" );
        out.println( "<body bgcolor=\"white\">" );
		
		out.println( "<center>" );
		out.println( "<h2>Please Wait</h2>" );
		out.println( "<font size=\"2\">" );
		out.println( "Your report is being generated. This can take up to 30 seconds.<br>" );
		out.println( "Do <b>not</b> press Refresh, Back, or any other command on your browser.<br>" );
		out.println( "Once the report is complete it will display here." );
		out.println( "</font>" );
		out.println( "<p>" );

		out.println( "<font color=red>" );
		out.println( "processing..." );
		out.println( "</font>" );

		out.println( "</center>" );

		// end boilerplate
		out.println( "<p>" );
		out.println( "<font size=\"-2\">" );
		out.println( "Copyright &copy; " + Calendar.getInstance().get(Calendar.YEAR) + " by Windward Studios, Inc. All Rights Reserved" );
		out.println( "</font>" );
		out.println( "</body>" );
		out.println( "</html>" );
	}
}
