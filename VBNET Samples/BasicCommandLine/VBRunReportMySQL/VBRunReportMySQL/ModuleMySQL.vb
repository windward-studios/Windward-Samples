Imports System
Imports System.Data
Imports Net.windward.api.csharp
Imports System.IO
Imports WindwardReportsDrivers.net.windward.datasource.ado
Imports WindwardInterfaces.net.windward.api.csharp

Module ModuleMySQL

    Sub Main()
        'Initilize the engine
        Report.Init()
        'open a inputfilestream for our template file
        Dim rtf As FileStream = File.OpenRead("../../../Samples/MySQL - Template.docx")
        'open an outputfilestream for our output
        Dim output As FileStream = File.Create("../../../Samples/MySql Report.pdf")
        'Create a report process
        Dim myReport As Report
        myReport = New ReportPdf(rtf, output)
        ' MySQL data source
        Dim strConn As String = "server=mysql.windward.net;database=sakila;user id=test;password=test;"
        Dim data As IReportDataSource = New AdoDataSourceImpl("MySql.Data.MySqlClient", strConn)

        'run the report process
        myReport.ProcessSetup()
        'the second parameter is the name of the data source
        myReport.ProcessData(data, "MYSQL")
        myReport.ProcessComplete()

        'close out of our template file and output
        output.Close()
        rtf.Close()

        'Open finished report
        Dim fullPath As String = Path.GetFullPath("../../../Samples/MySql Report.pdf")
        System.Diagnostics.Process.Start(fullPath)
    End Sub

End Module
