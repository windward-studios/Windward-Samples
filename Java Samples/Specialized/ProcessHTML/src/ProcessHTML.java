/*
* Copyright (c) 2012 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.xmlreport.ProcessHtml;
import net.windward.xmlreport.ProcessPdf;
import net.windward.xmlreport.ProcessPdfAPI;
import net.windward.xmlreport.ProcessReport;
import net.windward.env.SystemWrapper;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.HashMap;
import java.util.Map;

public class ProcessHTML
{
    public static void main(String[] args) throws Exception
    {
            // Initialize Windward Reports
            ProcessReport.init();
            File outFile = new File("ProcessHTML report test.html");
            FileOutputStream out = new FileOutputStream(outFile);
            String filename= new File("HTML Example.docx").getAbsolutePath();
            FileInputStream rtf = new FileInputStream(filename);

            //open our xml file and create a datasource
            InputStream dataSourceStream = new FileInputStream(new File("Windward Trucking 2 - Data.xml").getAbsolutePath());
            Dom4jDataSource data = new Dom4jDataSource(dataSourceStream);

            // Create a report process
            ProcessHtml proc = new ProcessHtml( rtf, out );

            /**
             *  Here is an overview of calls you can make to change the format of HTML output
             *  See ProcessHtmlAPI on our wiki at http://wiki.windward.net/Java_Engine/Java_Engine_API
             *  for more information
             *  **/

            //This disables the default page width which puts the html in a container
            //instead it just stretches out across the entire page
            proc.setUseTemplatePageWidth(false);

            //this enables the engine to carry across the first header and footer it finds
            proc.setHeadersFooters(true);

            String dir = new File(".").getAbsolutePath();

            //this call directs where to put images and where the html file should look for them
            proc.setImagePath(dir,dir,"WindwardSample");


            //This will include the css file separately from the html output
            //You can also use CSS_EXISTS if it has already been generated,
            //CSS_NO if don't want any css generated and
            //CSS_INCLUDE for the default where it is included in the html output
            String cssFileName= "Windward.css";
            FileOutputStream cssOut = new FileOutputStream(cssFileName);
            proc.setCss(ProcessHtml.CSS_SEPARATE,cssOut,cssFileName);

            // parse the template file
            System.out.println("Generating report...");
            proc.processSetup();
            
			// merge a sql database with the report
            proc.processData(data, "FD");
            
			// generate the final report
            proc.processComplete();
            
			// ensure everything is written out to the stream
            out.flush();
			out.close();
			cssOut.close();
			rtf.close();
			dataSourceStream.close();

			// Open the report
			System.out.println("Launching report " + outFile.getAbsolutePath());
            SystemWrapper.LaunchFile(outFile.getAbsolutePath());

        }
}