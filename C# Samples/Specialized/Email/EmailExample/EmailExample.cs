using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;
using System.Net;
using System.Net.Mail;


namespace EmailExample
{
    class EmailExample
    {
        // You must set these values to run this sample
        private static String fromEmailAddress = "";
        private static String fromPassword = ""; // only needed if relaying (server != to addreess)
        private static String emailServer = ""; // generally use the server for the toEmailAddress
        private static String toEmailAddress = "";

        static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(fromEmailAddress) || string.IsNullOrEmpty(toEmailAddress) || string.IsNullOrEmpty(emailServer))
            {
                Console.Error.WriteLine("please enter values for email address, password, server, etc.");
                Console.Out.WriteLine("press any key to cancel program");
                Console.ReadKey();
                return;
            }

            const string subject = "Windward Reports Test Message";
            const string body = "The report is attached to this email";

            MailAddress fromAddress = new MailAddress(fromEmailAddress);
            MailAddress toAddress = new MailAddress(toEmailAddress);

            // gmail uses port 587
            SmtpClient smtp;
            if (emailServer.ToLower().Contains(".gmail."))
                smtp = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 10000
                };
            else
            {
                smtp = new SmtpClient(emailServer);
                if (!string.IsNullOrEmpty(fromPassword))
                    smtp.Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword);
            }


            // Initialize the engine
            Report.Init();

            // Open template file and create output stream
            FileStream template = File.OpenRead("../../../Samples/Email Example Template.docx");
            MemoryStream output = new MemoryStream();

            // Create report process
            ReportPdf myReport = new ReportPdf(template, output);


            // Open an inputfilestream for our data file
            FileStream Xml = File.OpenRead("../../../Samples/Windward Trucking 2 - Data.xml");

            // Open a data object to connect to our xml file
            IReportDataSource data = new XmlDataSourceImpl(Xml, false);

            // Run the report process
            myReport.ProcessSetup();
            // The second parameter is the name of the data source
            myReport.ProcessData(data, "FD");
            myReport.ProcessComplete();

            using (var message = new MailMessage(fromAddress, toAddress){ Subject = subject, Body = body})
            {
                // Sets up the name and file type of the stream
                System.Net.Mime.ContentType content = new System.Net.Mime.ContentType();
                content.MediaType = System.Net.Mime.MediaTypeNames.Application.Pdf;
                content.Name = "test.pdf";
                output.Position = 0;  // reset the position to the beginning of the stream - Don't forget!
                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(output, content);
                message.Attachments.Add(attachment); 
                
                smtp.Send(message);
            }

            //close out of our template file and output
            template.Close();
            Xml.Close();
            output.Close();
        }
    }
}
