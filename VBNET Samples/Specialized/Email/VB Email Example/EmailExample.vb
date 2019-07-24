Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Net.windward.api.csharp
Imports WindwardReportsDrivers.net.windward.datasource
Imports WindwardInterfaces.net.windward.api.csharp
Imports System.IO
Imports System.Net
Imports System.Net.Mail

Module EmailExample
    'You must set these values to run this sample
    Const fromEmailAddress As String = ""
    Const password As String = "" 'only needed if relaying (server != to addreess)
    Const toEmailAddress As String = "" 'generally use the server for the toEmailAddress
    Const emailServer As String = "" 'smtp.gmail.com for gmail

    Sub Main()


        If (String.IsNullOrEmpty(fromEmailAddress) Or String.IsNullOrEmpty(toEmailAddress) Or String.IsNullOrEmpty(emailServer)) Then
            Console.Error.WriteLine("please enter values for email address, password, server, etc.")
            Console.Out.WriteLine("press any key to cancel program")
            Console.ReadKey()
            Return
        End If

        Dim fromAddress As MailAddress = New MailAddress(fromEmailAddress)
        Dim toMailAddress As MailAddress = New MailAddress(toEmailAddress)

        Const subject As String = "Windward Reports Test Message"
        Const body As String = "The report is attached to this email"

        ' gmail uses port 587
        Dim smtp As SmtpClient

        If (emailServer.ToLower().Contains(".gmail.")) Then
            smtp = New SmtpClient()
            smtp.Host = "smtp.gmail.com"
            smtp.Port = 587
            smtp.EnableSsl = True
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network
            smtp.UseDefaultCredentials = False
            smtp.Credentials = New NetworkCredential(fromAddress.Address, password)
            smtp.Timeout = 10000

        Else
            smtp = New SmtpClient(emailServer)
            If (Not String.IsNullOrEmpty(password)) Then
                smtp.Credentials = New System.Net.NetworkCredential(fromAddress.Address, password)
            End If
        End If


        ' Initialize the engine
        Report.Init()

        ' Open template file and create output stream
        Dim template As FileStream = File.OpenRead("../../../Samples/Email Example Template.docx")
        Dim output As MemoryStream = New MemoryStream()

        ' Create report process
        Dim myReport As ReportPdf = New ReportPdf(template, output)


        ' Open an inputfilestream for our data file
        Dim Xml As FileStream = File.OpenRead("../../../Samples/Windward Trucking 2 - Data.xml")

        ' Open a data object to connect to our xml file
        Dim Data As IReportDataSource = New XmlDataSourceImpl(Xml, False)

        ' Run the report process
        myReport.ProcessSetup()
        ' The second parameter is the name of the data source
        myReport.ProcessData(Data, "FD")
        myReport.ProcessComplete()

        Using message As MailMessage = New MailMessage(fromAddress, toMailAddress)
            message.Subject = subject
            message.Body = body

            ' Sets up the name and file type of the stream
            Dim content As System.Net.Mime.ContentType = New System.Net.Mime.ContentType()
            content.MediaType = System.Net.Mime.MediaTypeNames.Application.Pdf
            content.Name = "test.pdf"
            output.Position = 0  ' reset the position to the beginning of the stream - Don't forget!
            Dim Attachment As System.Net.Mail.Attachment = New System.Net.Mail.Attachment(output, content)
            message.Attachments.Add(Attachment)

            smtp.Send(message)
        End Using

        'close out of our template file and output
        template.Close()
        Xml.Close()
        output.Close()
    End Sub

End Module
