Imports net.windward.api.csharp
Imports WindwardInterfaces.net.windward.api.csharp
Imports System.IO

Public Class DisplayReport
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim basePath As String = Request.PhysicalApplicationPath + "\\"

        ' Initialize the engine. License and configuration settings in web.config.
        Report.Init()

        ' Open template file
        Dim template As FileStream = File.OpenRead(basePath + "Microsoft Access Datasource Connection - Template.docx")

        ' Create report process
        Dim myReport As Report = New ReportPdf(template)

        ' The data is stored in the Samples folder
        Dim fullPathData As String = basePath + "Northwind - Data.mdb"

        ' Access data source
        Dim strConn As String = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=" + fullPathData
        Dim data As IReportDataSource = New AdoDataSourceImpl("System.Data.Odbc", strConn)

        ' Run the report process
        myReport.ProcessSetup()
        ' the second parameter is the name of the data source
        myReport.ProcessData(data, "NWMINIACCESS")
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