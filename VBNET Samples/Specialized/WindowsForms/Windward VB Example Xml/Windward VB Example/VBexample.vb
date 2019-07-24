

Imports System
Imports System.Data
Imports net.windward.api.csharp
Imports System.IO
Imports WindwardReportsDrivers.net.windward.datasource.ado
Imports WindwardInterfaces.net.windward.api.csharp


Public Class VBexample

    Public Sub XmlReport()
        'Initilize the engine
        Report.Init()
        'open a inputfilestream for our template file
        Dim rtf As FileStream = File.OpenRead("../../../Samples/Windward Trucking 2 - Template.docx")
        'open an inputfilestream for our data file
        Dim Xml As FileStream = File.OpenRead("../../../Samples/Windward Trucking 2 - Data.xml")
        'open an outputfilestream for our output
        Dim output As FileStream = File.Create("../../../Samples/Xml Report.pdf")

        'Create a report process
        Dim myReport As Report = New ReportPdf(rtf, output)

        'open a data object to connect to our xml file
        Dim data As IReportDataSource = New XmlDataSourceImpl(Xml, False)

        'run the report process
        myReport.ProcessSetup()
        'the second parameter is "" to tell the process that our data is the default data source
        myReport.ProcessData(data, "")
        myReport.ProcessComplete()

        'close out of our template file and output
        output.Close()
        rtf.Close()
        Xml.Close()

        'Open finished report
        Dim fullPath As String = Path.GetFullPath("../../../Samples/Xml Report.pdf")
        System.Diagnostics.Process.Start(fullPath)

        Return
    End Sub

    'xml generate button
    Private Sub GenerateXml_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles GenerateXml.Click
        GenerateXml.Enabled = False
        XmlReport()
        GenerateXml.Enabled = True
    End Sub

End Class
