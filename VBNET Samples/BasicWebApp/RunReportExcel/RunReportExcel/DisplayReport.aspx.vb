﻿Imports net.windward.api.csharp
Imports WindwardInterfaces.net.windward.api.csharp
Imports System.IO

Public Class DisplayReport
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim basePath As String = Request.PhysicalApplicationPath + "\\"

        ' Initialize the engine. License and configuration settings in web.config.
        Report.Init()

        ' Open template file
        Dim template As FileStream = File.OpenRead(basePath + "Microsoft Excel File Datasource - Template.docx")

        ' Create report process
        Dim myReport As Report = New ReportPdf(template)

        ' The data is stored in the Samples folder
        Dim fullPathData As String = basePath + "Northwind Mini - Data.xlsx"

        ' Excel data source
        Dim strConn As String = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPathData + ";Extended Properties=""Excel 12.0 Xml;HDR=YES"""
        Dim data As IReportDataSource = New AdoDataSourceImpl("System.Data.OleDb", strConn)

        ' Run the report process
        myReport.ProcessSetup()
        ' the second parameter is the name of the data source
        myReport.ProcessData(data, "NWMINIXL")
        myReport.ProcessComplete()

        ' Close out of our template file and output
        template.Close()

        ' Opens the finished report
        'Response.ContentType = "application/pdf" ' this would have the pdf open in the browser (disable content-disposition:attachment if you want this)
        Response.ContentType = "application/save" ' this is used with content-disposition to give the proper name of the file
        Response.AppendHeader("content-disposition", "attachment; filename=""Report.pdf""")
        Response.BinaryWrite(DirectCast(myReport.GetReport(), MemoryStream).ToArray())
        Response.End() ' Must be called for MS Office documents
    End Sub

End Class