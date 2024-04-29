/*
* Copyright (c) 2011 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.xmlreport.*;

import javax.activation.DataHandler;
import javax.activation.DataSource;
import javax.mail.*;
import javax.mail.internet.InternetAddress;
import javax.mail.internet.MimeBodyPart;
import javax.mail.internet.MimeMessage;
import javax.mail.internet.MimeMultipart;
import javax.mail.util.ByteArrayDataSource;
import java.io.*;
import java.util.Properties;

public class WindwardReportsEmail {
    public static void main(String[] args) throws Exception {

        // set these. And if you are not using gmail, set the properties below too.
        final String username = "your email here";
        final String password = "your password here";
        final String to = "who you are sending the email to";

		// remind to set email username/password above
		if (username.contains(" ") || ! username.contains("@"))        {
            System.out.println("Please set your username and password above");
            return;
        }

        //this sample is set up to use gmail with smtp you can set it up
        //to use other email servers
        Properties props = new Properties();
        props.put("mail.smtp.host", "smtp.gmail.com");
        props.put("mail.smtp.port", "587");
        props.put("mail.smtp.auth", "true");
        props.put("mail.smtp.starttls.enable", "true");

        Session session = Session.getInstance(props,
                new javax.mail.Authenticator() {
                    protected javax.mail.PasswordAuthentication getPasswordAuthentication() {
                        return new PasswordAuthentication(username, password);
                    }
                });

        // Initialize Windward Reports
        ProcessReport.init();
        ByteArrayOutputStream out = new ByteArrayOutputStream();
        String filename = new File("Email Example Template.docx").getAbsolutePath();
        FileInputStream rtf = new FileInputStream(filename);

        //open our xml file and create a datasource
        InputStream dataSourceStream = new FileInputStream(new File("Windward Trucking 2 - Data.xml").getAbsolutePath());
        Dom4jDataSource data = new Dom4jDataSource(dataSourceStream);

        // Create a report process
        ProcessPdfAPI proc = new ProcessPdf(rtf, out);
        // set the subject of the report
        proc.setSubject("An Acme, Inc. Report");
        // parse the template file
        System.out.println("Generating report...");
        proc.processSetup();
        // process xml source
        proc.processData(data, "FD");
        // generate the final report
        proc.processComplete();
        System.out.println("Done!");
        // ensure everything is written out to the stream
        out.flush();

        try {

            Message message = new MimeMessage(session);
            message.setFrom(new InternetAddress(username));
            message.setRecipients(Message.RecipientType.TO, InternetAddress.parse(to));
            message.setSubject("Windward Reports test Message");

            MimeBodyPart body = new MimeBodyPart();

            //message text
            body.setText("The Report is attached to this email");

            Multipart messageParts = new MimeMultipart();
            messageParts.addBodyPart(body);
            body = new MimeBodyPart();

            //break into parts and attach our report
            DataSource attachment = new ByteArrayDataSource(out.toByteArray(), "text/pdf");
            body.setDataHandler(new DataHandler(attachment));

            body.setFileName("test.pdf");
            messageParts.addBodyPart(body);

            message.setContent(messageParts);
            Transport.send(message);


        } catch (MessagingException e) {
            throw new RuntimeException(e);
        }

		out.close();
		dataSourceStream.close();
    }


}

