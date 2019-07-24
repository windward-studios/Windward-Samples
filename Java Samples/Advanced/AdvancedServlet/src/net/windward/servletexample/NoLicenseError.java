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
import java.io.IOException;
import java.io.PrintWriter;

/**
 * This class displays the error if a file could not be found.
 *
 * @author David Thielen
 * @version 1.0  March 20, 2003
 */

public class NoLicenseError extends HttpServlet {

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

		response.setContentType("text/html");
    	PrintWriter out = response.getWriter();

    	out.println("<html>");
        out.println("<head>");

		out.println( "<title>Windward demo - No License Exception</title>" );
    	out.println( "</head>" );
	    out.println( "<body bgcolor=\"white\">" );

		out.println( "<h1>License Exception</h1>" );

		out.println( "<h2>Error</h2>" );
		out.println( request.getAttribute( "javax.servlet.error.message" ) + "<br>" );
	
		out.println( "<h2>WindwardReports.properties location</h2>" );
		ServletContext context = getServletContext();
		String propFile = context.getInitParameter( "PropFile" );
		if (propFile == null)
			propFile = ".";
		propFile = context.getRealPath( propFile );
		out.println( propFile + "<br>" );
	
		out.println( "<h2>License Exception</h2>" );
		out.println( request.getAttribute( "javax.servlet.error.exception" ).toString() + "<br>" );
		
		out.println("</body>");
    	out.println("</html>");
    }
}



