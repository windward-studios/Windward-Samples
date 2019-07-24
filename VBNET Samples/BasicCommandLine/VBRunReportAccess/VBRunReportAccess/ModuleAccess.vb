Imports System
Imports System.Data
Imports net.windward.api.csharp
Imports System.IO
Imports WindwardReportsDrivers.net.windward.datasource.ado
Imports WindwardInterfaces.net.windward.api.csharp

Module ModuleAccess

    Sub Main()
        'Initilize the engine
        Report.Init()
        'open a inputfilestream for our template file
        Dim rtf As FileStream = File.OpenRead("../../../Samples/Microsoft Access Datasource Connection - Template.docx")
        'open an outputfilestream for our output
        Dim output As FileStream = File.Create("../../../Samples/Access Report.pdf")
        'Create a report process
        Dim myReport As Report
        myReport = New ReportPdf(rtf, output)

        ' The data is stored in the Samples folder
        Dim fullPathData As String = Path.GetFullPath("../../../Samples/Northwind - Data.mdb")

        'open a data object to connect to an sql server
        Dim strConn As String = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=" + fullPathData
        Dim data As IReportDataSource = New AdoDataSourceImpl("System.Data.Odbc", strConn)

        'run the report process
        myReport.ProcessSetup()
        'the second parameter is the name of the data source
        myReport.ProcessData(data, "NWMINIACCESS")
        myReport.ProcessComplete()

        'close out of our template file and output
        output.Close()
        rtf.Close()

        'Open finished report
        Dim fullPath As String = Path.GetFullPath("../../../Samples/Access Report.pdf")
        System.Diagnostics.Process.Start(fullPath)
    End Sub

End Module
