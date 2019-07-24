package test;

import client.*;
import org.junit.Test;

import java.io.*;
import java.net.URL;
import java.util.Arrays;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;

import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;

public class TestJavaClient {
       String URL_PATH = "http://localhost:49731/";

    @Test
    public void testClient_PostTemplateReturnsReportPdf() throws Exception {
        //set paths
        String templatePath = "samples\\Sample1.docx";
        //run report
        URL uri = new URL(URL_PATH);
        ByteArrayOutputStream output = new ByteArrayOutputStream();
        ReportPdf report = new ReportPdf(uri,new FileInputStream(new File(templatePath)),output );
        report.process();
        byte [] outputArray = output.toByteArray();
        assertTrue(outputArray.length > 8);
        // Compare the first 8 bytes to the '%PDF-1.5' literal
        byte[] buffer = new byte[8];
        buffer = Arrays.copyOf(outputArray, 8);
        assertTrue(Arrays.equals(buffer, new byte[]{0x25, 0x50, 0x44, 0x46, 0x2d, 0x31, 0x2e, 0x35}));
    }

    @Test
    public void testClient_PostTemplateWithXmlData()throws  Exception{
        //set paths
        String templatePath = "samples\\Manufacturing.docx";
        String dataSourcePath = "samples\\Manufacturing.xml";
        String outputPath = "samples\\XmlDataOutput.pdf";
        //set data sources
        HashMap<String, DataSource> dataSources = new HashMap<String, DataSource>();
        dataSources.put("MANF_DATA_2009",new XmlDataSource(new FileInputStream(new File(dataSourcePath))));
        //run report
        URL uri = new URL(URL_PATH);
        OutputStream output = new FileOutputStream(new File(outputPath));
        ReportPdf report = new ReportPdf(uri,new FileInputStream(new File(templatePath)),output);
        report.process(dataSources);
    }

    @Test
    public  void testClient_VariablesTest()throws  Exception{
        //set paths
        String templatePath = "samples\\Variables.docx";
        String dataSourcePath = "samples\\Manufacturing.xml";
        String outputPath = "samples\\VariablesOutput.pdf";
        //set data sources
        HashMap<String, DataSource> dataSources = new HashMap<String, DataSource>();
        XmlDataSource xml = new XmlDataSource(new FileInputStream(new File(dataSourcePath)));
        List<TemplateVariable> list = new LinkedList<TemplateVariable>();
        list.add(new TemplateVariable("var1","value1"));
        xml.setVariables(list);
        dataSources.put("MANF_DATA_2009",xml);
        //run report
        URL uri = new URL(URL_PATH);
        OutputStream output = new FileOutputStream(new File(outputPath));
        ReportPdf report = new ReportPdf(uri,new FileInputStream(new File(templatePath)),output);
        report.process(dataSources);
    }

    @Test
    public void testClient_GetVersion()throws Exception{
          URL uri = new URL(URL_PATH);
          Version v = Report.GetVersion(uri);
          assertNotNull(v);
          assertTrue(v.engineVersion instanceof String);
          assertTrue(v.engineVersion instanceof String);
    }

    @Test
    public void testClient_PostTemplateWithAdoData()throws  Exception{
        //set paths
        String templatePath = "samples\\MsSqlTemplate.docx";
        String outputPath = "samples\\AdoDataOutput.pdf";
        //set data sources
        HashMap<String, DataSource> dataSources = new HashMap<String, DataSource>();
        dataSources.put("MSSQL",new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo"));
        //run report
        URL uri = new URL(URL_PATH);
        OutputStream output = new FileOutputStream(new File(outputPath));
        ReportPdf report = new ReportPdf(uri,new FileInputStream(new File(templatePath)),output);
        report.process(dataSources);
    }

    @Test
    public void testClient_PostTemplateAsync()throws  Exception{
        //set paths
        String templatePath = "samples\\Manufacturing.docx";
        String dataSourcePath = "samples\\Manufacturing.xml";
        String outputPath = "samples\\AsyncOutput.pdf";
        //set data sources
        HashMap<String, DataSource> dataSources = new HashMap<String, DataSource>();
        dataSources.put("MANF_DATA_2009",new XmlDataSource(new FileInputStream(new File(dataSourcePath))));
        //run report
        URL uri = new URL(URL_PATH);
        OutputStream output = new FileOutputStream(new File(outputPath));
        ReportPdf report = new ReportPdf(uri,new FileInputStream(new File(templatePath)));
        report.process(dataSources);
        while (report.getStatus() == Report.Status.Working)
            Thread.sleep(100);
        if (report.getStatus() == Report.Status.Ready)
        {
            output.write(report.getReport());
            report.delete();
        }
    }

    /**
     * Tests the template with datasets.
     *
     * @throws Exception
     */
    @Test
    public void client_TestDatasets() throws Exception {
        HashMap<String, DataSource> dataSources = new HashMap<String, DataSource>();
        dataSources.put("",new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=AdventureWorks;User=demo;Password=demo"));

        String templatePath = "../SampleTemplates/DataSet.docx";
        String outputPath = "../SampleTemplates/DataSetOutput.pdf";
        String datasetFilePath = "../SampleTemplates/DataSet.rdlx";

        URL uri = new URL(URL_PATH);
        OutputStream output = new FileOutputStream(new File(outputPath));
        ReportPdf report = new ReportPdf(uri, new FileInputStream(new File(templatePath)), output);

        InputStream dataset = new FileInputStream(new File(datasetFilePath));
        report.setDatasets(new Dataset[] { new Dataset(dataset) });

        report.process(dataSources);
    }
}