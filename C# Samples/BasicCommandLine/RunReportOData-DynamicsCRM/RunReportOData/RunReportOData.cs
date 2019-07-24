using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;
using Kailua.net.windward.utils;
using WindwardReportsAPI.net.windward.api.csharp;
using WindwardReportsDrivers.net.windward.datasource.authentication;

namespace RunReportOData
{
    class RunReportOData
    {
        static String serviceUrl = "https://dynamics-crm.windward.net:33771/CRMTest/XRMServices/2011/OrganizationData.svc";
        static String resource = "https://dynamics-crm.windward.net:33771";
        static String authorityUrl = "https://dynamics-crm.windward.net/adfs/services/trust/13/usernamemixed";
        // OAuth prompt use Username:demo@dynamicstest.net Password:demo
        static String clientId = "abcde";
        static String redirectUri = "http://localhost:1337/myredirect";

        static void Main(string[] args)
        {
            // Initialize the engine
            Report.Init();

            // Open template file and create output file
            FileStream template = File.OpenRead("../../../Samples/MSDynamics CRM - Template.docx");
            FileStream output = File.Create("../../../Samples/OData Report.pdf");
            Report myReport = new ReportPdf(template, output);


            // To connect we first we need to grab the security access token, here we use Windward's example implementation
            // of WS-Trust authenticator and you can change AuthenticateWithWsTrust() to be AuthenticateWithOAuthPrompt() here.
            // Also you can replace the authenticator with your own implementation and pass it's token into WrCredentials instead.
            var authToken = WsTrustAuthenticate();

            var securityTokenCredentials = new WrCredentials { SecurityToken = authToken.AccessToken };

            var datasource = new ODataDataSourceImpl(serviceUrl, securityTokenCredentials, FileUtils.CONNECT_MODE.SECURITY_TOKEN, 2);

            // Run the report process
            myReport.ProcessSetup();
            // The second parameter is "" to tell the process that our data is the default data source
            myReport.ProcessData(datasource, "");
            myReport.ProcessComplete();

            // Close out of our template file and output
            output.Close();
            template.Close();

            // Opens the finished report
            string fullPath = Path.GetFullPath("../../../Samples/OData Report.pdf");
            System.Diagnostics.Process.Start(fullPath);
        }

        private static AuthenticationToken WsTrustAuthenticate()
        {
            var authenticator = new WsTrustAuthenticator(authorityUrl, resource);
            return authenticator.AquireAuthToken("demo@dynamicstest.net", "demo");
        }

        private static AuthenticationToken OAuthPromptAuthenticate()
        {
            var authenticator = new ActiveDirectoryOAuthenticator(authorityUrl);
            return authenticator.Authenticate(resource, clientId, redirectUri);
        }
    }
}
