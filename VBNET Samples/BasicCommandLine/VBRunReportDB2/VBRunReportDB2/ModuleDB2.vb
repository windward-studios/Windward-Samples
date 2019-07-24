Imports System
Imports System.Data
Imports net.windward.api.csharp
Imports System.IO
Imports WindwardReportsDrivers.net.windward.datasource.ado
Imports WindwardInterfaces.net.windward.api.csharp

Module ModuleDB2

    Sub Main()
        'Initilize the engine
        Report.Init()
        'open a inputfilestream for our template file
        Dim rtf As FileStream = File.OpenRead("../../../Samples/DB2 - Templates.xlsx")
        'open an outputfilestream for our output
        Dim output As FileStream = File.Create("../../../Samples/DB2 Report.pdf")
        'Create a report process
        Dim myReport As Report
        myReport = New ReportPdf(rtf, output)
        'open a data object to connect to an DB2 server
        Dim strConn As String = "server=db2.windward.net;database=Sample;User ID=demo;Password=demo;"
        Dim data As IReportDataSource = New AdoDataSourceImpl("IBM.Data.DB2", strConn)

        'run the report process
        myReport.ProcessSetup()
        'the second parameter is the name of the data source
        myReport.ProcessData(data, "DB2")
        myReport.ProcessComplete()

        'close out of our template file and output
        output.Close()
        rtf.Close()

        'Open finished report
        Dim fullPath As String = Path.GetFullPath("../../../Samples/DB2 Report.pdf")
        System.Diagnostics.Process.Start(fullPath)
    End Sub

End Module
