/*
* Copyright (c) 2003 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package com.windwardreports;

import java.io.*;
import java.text.*;
import java.util.*;
import jakarta.servlet.*;
import jakarta.servlet.http.*;


/**
 * This class displays the error if a file could not be found.
 *
 * @author David Thielen
 * @version 1.0  March 20, 2003
 */

public class NoFileError extends HttpServlet {

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

		out.println( "<title>Windward Reports demo - File Not Found Exception</title>" );
    	out.println( "</head>" );
	    out.println( "<body bgcolor=\"white\">" );

		out.println( "<h1>Could not find template or data file</h1>" );
		out.println( "<h2>Error</h2>" );
		out.println( (String) request.getAttribute( "javax.servlet.error.message" ) + "<br>" );

		out.println( "<h2>Exception</h2>" );
		out.println( request.getAttribute( "javax.servlet.error.exception" ).toString() + "<br>" );
		
		out.println("</body>");
    	out.println("</html>");
    }
}



