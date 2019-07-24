Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Net.windward.api.csharp
Imports WindwardReportsDrivers.net.windward.datasource
Imports WindwardInterfaces.net.windward.api.csharp
Imports System.IO

Module ModuleVariable

    Sub Main()
        ' Initialize the engine
        Report.Init()

        ' Open template file and create output file
        Dim template As FileStream = File.OpenRead("../../../Samples/Variable Invoice Sample - Template.docx")
        Dim output As FileStream = File.Create("../../../Samples/Variable Report.pdf")

        ' Create report process
        Dim myReport As Report = New ReportPdf(template, output)


        ' SQL data source
        Dim strConn As String = "Data Source=mssql.windwardreports.com;Initial Catalog=Northwind;User ID=demo;Password=demo;"
        Dim Data As IReportDataSource = New AdoDataSourceImpl("System.Data.SqlClient", strConn)

        'run the report process
        myReport.ProcessSetup()

        'This is where we pass in the parameters
        Dim map As Generic.Dictionary(Of String, Object) = New Dictionary(Of String, Object)
        'order is our variable
        map.Add("order", 10537)
        'This is the function where we actually tell our report the parameter values
        myReport.Parameters = map

        'the second parameter is the name of the data source
        myReport.ProcessData(Data, "MSSQL")
        myReport.ProcessComplete()

        'close out of our template file and output
        output.Close()
        template.Close()

        ' Open the finished report
        Dim fullPath As String = Path.GetFullPath("../../../Samples/Variable Report.pdf")
        System.Diagnostics.Process.Start(fullPath)
    End Sub

End Module
