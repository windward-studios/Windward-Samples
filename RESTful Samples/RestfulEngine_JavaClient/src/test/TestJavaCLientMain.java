package test;

import client.*;

import java.io.*;
import java.net.URL;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;

/**
 * Created by Bassem on 4/14/2015.
 */
public class TestJavaCLientMain {
    static String URL_PATH = "http://localhost/restfulengine/";

    public static void client_PostTemplateReturnsReportPdf()throws  Exception{
        String templatePath = "E:\\VSO\\14.0\\Merge\\RESTfulEngine\\TestFiles\\Sample1.docx";
        URL uri = new URL(URL_PATH);
        ByteArrayOutputStream output = new ByteArrayOutputStream();
        ReportPdf report = new ReportPdf(uri,new FileInputStream(new File(templatePath)),output );
        report.process();
        System.out.println(output.toByteArray().length);
    }

    public static void client_PostTemplateWithXmlData() throws Exception {
        String templatePath = "..\\..\\TestFiles\\Manufacturing.docx";
        String dataSourcePath = "..\\..\\TestFiles\\Manufacturing.xml";
        URL uri = new URL(URL_PATH);
        ReportPdf report = new ReportPdf(uri, new FileInputStream(new File(templatePath)));
        HashMap<String, DataSource> dataSources = new HashMap<String, DataSource>();
        dataSources.put("MANF_DATA_2009",new XmlDataSource(new FileInputStream(new File(dataSourcePath))));
        report.process(dataSources);
    }


    public static void client_PostTemplateWithAdoData()throws  Exception{
        String templatePath = "E:\\VSO\\14.0\\Merge\\RESTfulEngine\\TestFiles\\MsSqlTemplate.docx";
        URL uri = new URL(URL_PATH);
        ReportPdf report = new ReportPdf(uri,new FileInputStream(new File(templatePath)));
        HashMap<String, DataSource> dataSources = new HashMap<String, DataSource>();
        dataSources.put("MSSQL",new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo"));
        report.process(dataSources);
    }

    public static void client_VariablesTest()throws  Exception{
        String templatePath = "E:\\VSO\\14.0\\Merge\\RESTfulEngine\\TestFiles\\Manufacturing.docx";
        String dataSourcePath = "E:\\VSO\\14.0\\Merge\\RESTfulEngine\\TestFiles\\Manufacturing.xml";
        URL uri = new URL(URL_PATH);
        ReportPdf report = new ReportPdf(uri,new FileInputStream(new File(templatePath)));
        HashMap<String, DataSource> dataSources = new HashMap<String, DataSource>();
        XmlDataSource xml = new XmlDataSource(new FileInputStream(new File(dataSourcePath)));
        List<TemplateVariable> list = new LinkedList<TemplateVariable>();
        list.add(new TemplateVariable("var1","value1"));
        list.add(new TemplateVariable("var2","value2"));
        xml.setVariables(list);
        dataSources.put("MANF_DATA_2009",xml);
        report.process(dataSources);
    }

    public static void main(String []args) throws  Exception{
        client_PostTemplateWithXmlData();
    }
}
