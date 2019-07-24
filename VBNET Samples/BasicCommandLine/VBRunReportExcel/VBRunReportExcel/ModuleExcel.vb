Imports System
Imports System.Data
Imports Net.windward.api.csharp
Imports System.IO
Imports WindwardReportsDrivers.net.windward.datasource.ado
Imports WindwardInterfaces.net.windward.api.csharp

Module ModuleExcel

    Sub Main()
        'Initilize the engine
        Report.Init()
        'open a inputfilestream for our template file
        Dim rtf As FileStream = File.OpenRead("../../../Samples/Microsoft Excel File Datasource - Template.docx")
        'open an outputfilestream for our output
        Dim output As FileStream = File.Create("../../../Samples/Excel Report.pdf")
        'Create a report process
        Dim myReport As Report
        myReport = New ReportPdf(rtf, output)

        ' The data is stored in the Samples folder
        Dim fullPathData As String = Path.GetFullPath("../../../Samples/Northwind Mini - Data.xlsx")

        'open a data object to connect to an sql server
        Dim strConn As String = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPathData + ";Extended Properties=""Excel 12.0 Xml;HDR=YES"""
        Dim data As IReportDataSource = New AdoDataSourceImpl("System.Data.OleDb", strConn)

        'run the report process
        myReport.ProcessSetup()
        'the second parameter is the name of the data source
        myReport.ProcessData(data, "NWMINIXL")
        myReport.ProcessComplete()

        'close out of our template file and output
        output.Close()
        rtf.Close()

        'Open finished report
        Dim fullPath As String = Path.GetFullPath("../../../Samples/Excel Report.pdf")
        System.Diagnostics.Process.Start(fullPath)
    End Sub

End Module