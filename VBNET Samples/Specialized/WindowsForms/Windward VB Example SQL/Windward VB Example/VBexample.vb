Imports System
Imports System.Data
Imports net.windward.api.csharp
Imports System.IO
Imports WindwardReportsDrivers.net.windward.datasource.ado
Imports WindwardInterfaces.net.windward.api.csharp


Public Class VBexample

    Public Sub SqlReport()
        'Initilize the engine
        report.Init()
        'open a inputfilestream for our template file
        Dim rtf As FileStream = File.OpenRead("../../../Samples/Microsoft SQL Server - Template.docx")
        'open an outputfilestream for our output
        Dim output As FileStream = File.Create("../../../Samples/Sql Report.pdf")
        'Create a report process
        Dim myReport As Report
        myReport = New ReportPdf(rtf, output)
        'open a data object to connect to an sql server
        Dim strConn As String = "Data Source=mssql.windward.net;Initial Catalog=Northwind;User ID=demo;Password=demo;"
        Dim data As IReportDataSource = New AdoDataSourceImpl("System.Data.SqlClient", strConn)

        'run the report process
        myReport.ProcessSetup()
        'the second parameter is the name of the data source
        myReport.ProcessData(data, "MSSQL")
        myReport.ProcessComplete()

        'close out of our template file and output
        output.Close()
        rtf.Close()

        'Open finished report
        Dim fullPath As String = Path.GetFullPath("../../../Samples/Sql Report.pdf")
        System.Diagnostics.Process.Start(fullPath)

    End Sub

    'sql generate button
    Private Sub GenerateSql_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles GenerateSql.Click
        GenerateSql.Enabled = False
        SqlReport()
        GenerateSql.Enabled = True
    End Sub

End Class
