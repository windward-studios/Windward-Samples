/*
 * Copyright (c) 2012 by Windward Studios, Inc. All rights reserved.
 *
 * This program can be copied or used in any manner desired.
 */

import java.io.*;

import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.xml.SaxonDataSource;
import net.windward.xmlreport.ProcessPdf;
import net.windward.xmlreport.ProcessReportAPI;
import net.windward.env.SystemWrapper;

/*
 * A sample usage of the Windward Java Engine. This is the first building block to a great report!
 *
 * This sample takes in a datasource and template file--in this case, data.xml and template.docx from
 * the data directory--and produces the report, stored as report.pdf in the out directory.
 *
 * Take a look, the results are amazing!
 */
public class BasicXml {
    public static void main(String[] args) {
        try {
			// To generate a report, first we need a ProcessReport object.  For now, we're using the
            // pdf format to output.
            File fileReport = new File("out/report.pdf");
			FileInputStream template = new FileInputStream("data/template.docx");
			FileOutputStream reportStream = new FileOutputStream(fileReport);
            ProcessReportAPI report = new ProcessPdf(template, reportStream);
            
			// Preparation...
			report.processSetup();
            
			// Set up the datasource. The parameters are connector package, url, username, password.
            // For each type of datasource, the connector package is different
            DataSourceProvider datasource = new SaxonDataSource(new FileInputStream("data/data.xml"));
            
			// Finally, send it to Windward for processing.  The second parameter is the name of the
            // datasource.  This should match the name used in your template.
			report.processData(datasource, "Orders");
            
			// And... DONE!
			report.processComplete();
			template.close();
			reportStream.close();
            System.out.println("Launching report " + fileReport.getAbsolutePath());
            SystemWrapper.LaunchFile(fileReport.getAbsolutePath());
			
        } catch (Exception e) {
            // Uh oh, just in case
            e.printStackTrace();
        }
    }
}
