/*
* Copyright (c) 2017 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

package net.windward;

import java.io.*;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.xmlreport.ProcessPdf;
import net.windward.xmlreport.ProcessReportAPI;
import net.windward.xmlreport.errorhandling.ErrorInfo;
import net.windward.xmlreport.errorhandling.Issue;

public class Main {

/**
 * Sample document demonstrating how to include Error Handling into a java app and print the error stream to a .txt file
 *
 * @author Adam Austin
 */

    public static void main(String[] args) {
        try {
            // To generate a report, first we need a ProcessReport object.  For now, we're using the
            // pdf format to output.
            File fileReport = new File("out/report.pdf");
            FileInputStream template = new FileInputStream("data/Smart Energy Template.docx");
            FileOutputStream reportStream = new FileOutputStream(fileReport);
            ProcessReportAPI report = new ProcessPdf(template, reportStream);

            // Preparation...
            System.out.println("Generating report...");
            report.processSetup();

            // Set Track Verify and Error Handling issues during report generation based off a command line argument
            String trackErrrorSetting = "";
            if (args.length > 0) {
                trackErrrorSetting = args[0];
            }

            switch(trackErrrorSetting) {
                case("0"):
                    System.out.println("Track Errors: None");
                    report.setTrackErrors(ProcessReportAPI.ERROR_HANDLING_NONE);
                    break;
                case("1"):
                    System.out.println("Track Errors: Error Handling");
                    report.setTrackErrors(ProcessReportAPI.ERROR_HANDLING_TRACK_ERRORS);
                    break;
                case("2"):
                    System.out.println("Track Errors: Verify");
                    report.setTrackErrors(ProcessReportAPI.ERROR_HANDLING_VERIFY);
                    break;
                case("3"):
                default:
                    System.out.println("Track Errors: All");
                    report.setTrackErrors(ProcessReportAPI.ERROR_HANDLING_ALL);
                    break;
            }

            // Set up the data hash map.
            Map<String, DataSourceProvider> dataProviders = new HashMap<String, DataSourceProvider>();

            // Create an instance of DataSourceProvider
            DataSourceProvider datasource = new Dom4jDataSource(new FileInputStream("data/Smart Energy - Broken.xml"));

            // Add the data source to the data hash map
            dataProviders.put("", datasource);

            // Process the data stored in the hash map
            report.processData(dataProviders);

            // And... DONE!
            report.processComplete();
            template.close();
            reportStream.close();

            // Print errors found by Error Handling and Verify to the command line and the file "Issues.txt"
            PrintWriter writer = new PrintWriter("out/Issues.txt", "UTF-8");

            // Retrieve a list of issues encountered during report generation
            ErrorInfo outputIssues = report.getErrorInfo();
            List<Issue> errors = outputIssues.getErrors();

            System.out.println();
            System.out.println("---------------------------------------------------");
            System.out.println("Errors found during Verify upon Report Generation:");
            System.out.println("---------------------------------------------------");

            writer.println("---------------------------------------------------");
            writer.println("Errors found by Verify upon Report Generation:");
            writer.println("---------------------------------------------------");

            // Print every issue to the command line and the isseus.txt file
            for (int i = 0; i < errors.size(); i++) {
                System.out.println(errors.get(i).getMessage());
                writer.println(errors.get(i).getMessage());
            }
            writer.close();
        } catch (Exception e) {
            // Uh oh, just in case
            e.printStackTrace();
        }
    }
}