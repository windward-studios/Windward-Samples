/*
* Copyright (c) 2012 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

import net.windward.datasource.*;
import net.windward.datasource.dataset.DataSetDataSource;
import net.windward.datasource.xml.*;
import net.windward.datasource.jdbc.*;
import net.windward.env.SystemWrapper;
import net.windward.env.WindwardWrapper;
import net.windward.xmlreport.ProcessPdf;
import net.windward.xmlreport.ProcessReportAPI;
import net.windward.env.SystemWrapper;

import java.io.*;
import java.lang.reflect.Method;
import java.util.HashMap;
import java.util.Map;

public class RunUsingDatasets {

    public static void main(String[] args) throws Exception {

        try {
            try {
                Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver").newInstance();
            } catch (ClassNotFoundException e) {
                throw new ClassNotFoundException("Please add the SqlServer JDBC connector to your classpath. Details at http://rpt.me/SqlServerJDBC", e);
            }

            // To generate a report, first we need a ProcessReport object.
            // For now, we're using the pdf format to output.
            InputStream streamTemplate = new FileInputStream("files/Sample Dataset Template.docx");
            File reportFile = new File("out/Sample Dataset Report.pdf");
            OutputStream streamReport = new FileOutputStream(reportFile);


            ProcessReportAPI report = new ProcessPdf(streamTemplate, streamReport);

            // Preparation...
            System.out.println("Generating report...");
            report.processSetup();
            streamTemplate.close();

            // apply the XML data
            InputStream streamXmlData = new FileInputStream("files/SouthWind.xml");
            InputStream streamXmlSchema = new FileInputStream("files/SouthWind.xsd");
            DataSourceProvider dsSaxon = new SaxonDataSource(streamXmlData, streamXmlSchema);
            DataSetDataSource dsEmployeesUnder5 = new DataSetDataSource("employeesUnder5", "/windward-studios/Employees/Employee[@EmployeeID < 5]", dsSaxon);
            DataSetDataSource dsCustStartA = new DataSetDataSource("CustStartA", "/windward-studios/Customers/Customer[starts-with(CompanyName, 'A')]", dsSaxon);

            // apply the SQL data
            DataSourceProvider dsJdbc = new JdbcDataSource("com.microsoft.sqlserver.jdbc.SQLServerDriver",
                    "jdbc:sqlserver://mssql.windward.net;DatabaseName=Northwind", "demo", "demo");
            DataSetDataSource dsEmployeesMoreThan5 = new DataSetDataSource("EmpMoreThan5", "SELECT * FROM dbo.Employees WHERE(dbo.Employees.EmployeeID > 5)", dsJdbc);
            DataSetDataSource dsCustStartWithB = new DataSetDataSource("CustStartWithB", "SELECT * FROM dbo.Customers WHERE(dbo.Customers.CompanyName like 'B%')", dsJdbc);

            Map<String, DataSourceProvider> datasources = new HashMap<String, DataSourceProvider>();
            datasources.put("SW", dsSaxon);
            datasources.put("employeesUnder5", dsEmployeesUnder5);
            datasources.put("CustStartA", dsCustStartA);
            datasources.put("MSSQL", dsJdbc);
            datasources.put("EmpMoreThan5", dsEmployeesMoreThan5);
            datasources.put("CustStartWithB", dsCustStartWithB);
            report.processData(datasources);

            streamXmlData.close();
            streamXmlSchema.close();

            // And... DONE!
            report.processComplete();
            streamReport.close();

            // launch the generated report
            System.out.println("all threads completed");
            System.out.println("Launching report " + reportFile.getAbsolutePath());
            SystemWrapper.LaunchFile(reportFile.getAbsolutePath());

        } catch (Exception e) {
            // Uh oh, just in case
            e.printStackTrace();
        }
    }
}
