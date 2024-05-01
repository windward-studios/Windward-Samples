/*
 * Copyright (c) 2018 by Windward Studios, Inc. All rights reserved.
 *
 * This program can be copied or used in any manner desired.
 */

import java.io.*;
import java.util.HashMap;
import java.util.Map;
import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.jdbc.JdbcDataSource;
import net.windward.datasource.odata.ODataDataSource;
import net.windward.xmlreport.ProcessPdf;
import net.windward.xmlreport.ProcessReport;
import net.windward.xmlreport.ProcessReportAPI;
import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.datasource.json.JsonDataSource;
import net.windward.datasource.abstract_datasource.salesforce.SalesForceDataSource;

public class BasicSalesforce {

    /**
    * Normally you will copy the code in this method to your application code.
    * @param args unused
    */

    public static void main(String[] args) throws Exception {

        // Initialize the engine.
        ProcessReport.init();

        // Read the template file.
        FileInputStream template = new FileInputStream("data/template.docx");

        // Create the generated report file.
        FileOutputStream reportStream = new FileOutputStream("out/report.pdf");

        // Pass the 2 streams to the object that will create a PDF report.
        // For other output types, you create a different object at this step.
        ProcessReportAPI myReport = new ProcessPdf(template, reportStream);

        // Read in the template and prepare it to merge the data
        myReport.processSetup();

        // Place all variables in this map. We assign this map to all datasources.
        Map<String, Object> mapVariables = new HashMap<String, Object>();
        // add variables here using: mapVariables.put("key", value);

        Map<String, DataSourceProvider> dataSources = new HashMap<String, DataSourceProvider>();

        DataSourceProvider sfdemo = new SalesForceDataSource("demo@windward.net", "w1ndw@rd", "BtqoH7pIR6rkR0fwh1YU156Hp", true);
        sfdemo.setMap(mapVariables);
        dataSources.put("sfdemo", sfdemo);
 
        // Insert all data into the report.
        myReport.processData(dataSources);
 
        // You should place the following in a finally block (we did not to keep this clear).
        sfdemo.close();

        // Write the generated report to the PDF file.
        myReport.processComplete();

        //ensure everything is written out to the stream
        template.close();

        reportStream.close();

    }

}