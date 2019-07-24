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
import java.io.IOException;
import java.io.PrintWriter;

/**
 * Parameter error page for parameter errors on post methods
 */
public class ParameterError extends HttpServlet {
    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
        doGet(req, resp);
    }

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
        resp.setContentType("text/html");
        PrintWriter out = resp.getWriter();

        out.println("<html>");

        out.println("<head>");
        out.println( "<title>Windward for Google App Engine demo - Parameter Error</title>" );
        out.println( "</head>" );

        out.println("<body>");

        out.println( "<h1>Parameter Exception</h1>" );

        out.println( "<h2>Error</h2>" );
        out.println(req.getAttribute("javax.servlet.error.message") + "<br>" );

        if (req.getAttribute("javax.servlet.error.exception") != null) {
            out.println("<h2>Caused By:</h2>");
            out.println(req.getAttribute("javax.servlet.error.exception").toString() + "<br>");
        }

        out.println("</body>");
        out.println("</html>");
    }
}
