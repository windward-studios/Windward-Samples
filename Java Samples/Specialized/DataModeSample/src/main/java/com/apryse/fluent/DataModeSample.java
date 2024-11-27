/*
 * Copyright (c) 2012 by Windward Studios, Inc. All rights reserved.
 *
 * This program can be copied or used in any manner desired.
 */
package com.apryse.fluent;

import java.io.*;

import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.xml.SaxonDataSource;
import net.windward.xmlreport.ProcessDocx;
import net.windward.xmlreport.ProcessReportAPI;

/*
 * A sample usage of the Data Mode Functionality with the Fluent Java Engine.
 *
 * This sample takes in a datasource and template file--in this case, datasource.xml and template.docx from
 * the data directory--and produces the report, stored as report.pdf in the out directory as well as produces
 * a output_data_*.xml file which contains the data as a datasource generated from the report.
 *
 * Take a look, the results are amazing!
 */
public class DataModeSample {
    public static void main(String[] args) {
        try {
            // To generate a report, first we need a ProcessReport object.  For now, we're using the
            // DOCX format to output.
            File fileReport = new File("output\\output.docx");
            FileInputStream template = new FileInputStream("assets\\DataModeTemplate.docx");
            FileOutputStream reportStream = new FileOutputStream(fileReport);
            ProcessDocx report = new ProcessDocx(template, reportStream);

            // Create data file stream and link it to the report data stream
            FileOutputStream dataFileStream = new FileOutputStream("output\\output_data.xml");
            report.setDataStream(dataFileStream);

            /**
             * The different data mode options (uncomment one of the following options)
             * ---------------------------------------------------------------------------
             */
            // Sets the data file to contain the returned data from the tags in the DataModeTemplate
            report.setDataMode(ProcessReportAPI.DATA_MODE_DATA);

            // Sets the data file to contain the select attributes from the tags in the DataModeTemplate
            //report.setDataMode(ProcessReportAPI.DATA_MODE_SELECT);

            // Sets the data file to contain all attributes from the tags in the DataModeTemplate
            //report.setDataMode(ProcessReportAPI.DATA_MODE_ALL_ATTRIBUTES);

            // Sets the data file to contain the data (uuencoded) from bitmap tags in the DataModeTemplate
            //report.setDataMode(ProcessReportAPI.DATA_MODE_INCLUDE_BITMAPS);

            // Sets the data file to contain all the information from the tags and data in the DataModeTemplate
            //report.setDataMode(ProcessReportAPI.DATA_MODE_DATA | ProcessReportAPI.DATA_MODE_SELECT | ProcessReportAPI.DATA_MODE_ALL_ATTRIBUTES | ProcessReportAPI.DATA_MODE_INCLUDE_BITMAPS);

            // Embeds the data file within the DOCX file and sets the data file to contain the select attributes from the tags in the DataModeTemplate
            //report.setDataMode(ProcessReportAPI.DATA_MODE_EMBED | ProcessReportAPI.DATA_MODE_DATA);
            /**
             * ---------------------------------------------------------------------------
             */

            // Preparation...
            report.processSetup();

            // Set up the datasource. The parameters are connector package, url, username, password.
            // For each type of datasource, the connector package is different
            DataSourceProvider datasource = new SaxonDataSource(new FileInputStream("assets\\DataSource.xml"));

            // Finally, send it to Fluent for processing.  The second parameter is the name of the
            // datasource.  This should match the name used in your template.
            report.processData(datasource, "SW");

            // And... DONE!
            report.processComplete();
            template.close();
            reportStream.close();

            System.out.println("Finished generating report: " + fileReport.getAbsolutePath());
            System.out.println("Finished generating data file: " + fileReport.getAbsolutePath());
        } catch (Exception e) {
            // Uh oh, just in case
            e.printStackTrace();
        }
    }
}
