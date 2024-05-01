/*
* Copyright (c) 2012 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/


import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.datasource.jdbc.JdbcDataSource;
import net.windward.xmlreport.ProcessPdf;
import net.windward.xmlreport.ProcessPdfAPI;
import net.windward.xmlreport.ProcessReport;
import net.windward.xmlreport.SetupException;

import javax.swing.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.util.HashMap;
import java.util.Map;

public class WindwardReportsGUI extends JFrame {

    public SwingForm form;

    public WindwardReportsGUI(){
        //create the design form and set it as our content pane
        form = new SwingForm();
        this.setContentPane(form.root);

        //action listener for XML button, runs report thread when called
        form.XML.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                XMLworker xml = new XMLworker();
                try {
                    xml.execute();
                } catch (Exception ee) {
                    ee.printStackTrace();
                }
            }
        });

        //action listener for MySQL button, runs report thread when called
        form.MySql.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                MYSQLworker mysql = new MYSQLworker();
                mysql.execute();
            }
        });

        form.DB2.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                DB2worker db2 = new DB2worker();
                db2.execute();
            }
        });

        form.MSSQL.addActionListener(new ActionListener() {
            public void actionPerformed(ActionEvent e) {
                MSSQLworker mssql = new MSSQLworker();
                mssql.execute();
            }
        });
    }

    class XMLworker extends SwingWorker {
        public Boolean doInBackground() throws Exception {
            form.progressBar.setIndeterminate(true);
            form.Status.setText("Processing");
            // Initialize Windward Reports
            ProcessReport.init();
            FileOutputStream out = new FileOutputStream("XML Report test.pdf");
            String filename = new File("Internet Marketing Report - Template.docx").getAbsolutePath();
            FileInputStream rtf = new FileInputStream(filename);

            //open our xml file and create a datasource
            InputStream dataSourceStream = new FileInputStream(new File("Internet Marketing - Data.xml").getAbsolutePath());
            Dom4jDataSource data = null;
            data = new Dom4jDataSource(dataSourceStream);

            // Create a report process
            ProcessPdfAPI proc = new ProcessPdf(rtf, out);
            // set the subject
            proc.setSubject("An Acme, Inc. Report");
            // parse the template file
            form.Status.setText("Generating report...");
            proc.processSetup();

            // merge a sql database with the report
            proc.processData(data, "INTMARKETING");
            // generate the final report
            proc.processComplete();
            // ensure everything is written out to the stream
            out.flush();
            out.close();
            form.Status.setText("Done");
            form.progressBar.setIndeterminate(false);
            return true;
        }

        public void done() {

        }
    }

    abstract class SQLworker extends SwingWorker {
        protected String driver;
        protected String url;
        protected String dbName;
        protected String reportName;
        protected String templateName;

        public Boolean doInBackground() throws Exception {
            form.progressBar.setIndeterminate(true);
            form.Status.setText("Processing");
            // Initialize Windward Reports
            ProcessReport.init();
            FileOutputStream out = new FileOutputStream(reportName);
            String filename = new File(templateName).getAbsolutePath();
            FileInputStream rtf = new FileInputStream(filename);

            //open our xml file and create a datasource
            JdbcDataSource data = null;
            try {
                data = new JdbcDataSource(this.driver, this.url, "demo", "demo");
            } catch (Exception ee) {
                form.Status.setText("Failed Check Console");
                form.progressBar.setIndeterminate(false);
                ee.printStackTrace();
            }
            // Create a report process
            ProcessPdfAPI proc = new ProcessPdf(rtf, out);
            // set the subject
            proc.setSubject("An Acme, Inc. Report");
            // parse the template file
            form.Status.setText("Generating report...");
            proc.processSetup();
            // merge a sql database with the report
            proc.processData(data, this.dbName);
            // generate the final report
            proc.processComplete();
            // ensure everything is written out to the stream
            out.flush();
            out.close();
            form.Status.setText("Done");
            form.progressBar.setIndeterminate(false);
            return true;
        }

        public void done() {

        }
    }

    class DB2worker extends SQLworker {
        DB2worker() {
            super();
            this.driver = "com.ibm.db2.jcc.DB2Driver";
            this.url = "jdbc:db2://db2.windward.net:50000/SAMPLE";
            this.dbName = "DB2";
            this.reportName = "DB2 Report Test.pdf";
            this.templateName = "DB2 - Template.xlsx";
        }
    }

    class MSSQLworker extends SQLworker {
        MSSQLworker() {
            super();
            this.driver = "com.microsoft.sqlserver.jdbc.SQLServerDriver";
            this.url = "jdbc:sqlserver://mssql.windward.net;DatabaseName=Northwind";
            this.dbName = "MSSQL";
            this.reportName = "MS SQL Report Test.pdf";
            this.templateName = "MSSQL - Template.docx";
        }
    }

    class MYSQLworker extends SQLworker {
        MYSQLworker() {
            super();
            this.driver = "com.mysql.jdbc.Driver";
            this.url = "jdbc:mysql://mysql.windward.net/sakila";
            this.dbName = "MYSQL";
            this.reportName = "MySQL Report Test.pdf";
            this.templateName = "MySQL - Template.docx";
        }
    }

    public static void main(String[] args) throws Exception {
        SwingUtilities.invokeLater(new Runnable() {
            public void run() {
                WindwardReportsGUI main = new WindwardReportsGUI();
                main.setDefaultCloseOperation(JFrame.DISPOSE_ON_CLOSE);
                main.setSize(325, 110);
                main.setVisible(true);
                main.setResizable(false);
                main.setTitle("Windward Basic Swing Sample");
            }
        });
    }


}
