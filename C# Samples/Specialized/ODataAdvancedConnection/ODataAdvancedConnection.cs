using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using WindwardInterfaces.net.windward.AdvancedConnection;

//using System;
//using System.Collections.Generic;
//using System.Net;
//using kailua.net.windward.utils;
//using log4net.Config;
//using WindwardInterfaces.net.windward.api.csharp;
//using net.windward.api.csharp;
//using System.IO;
//using WindwardInterfaces.net.windward.AdvancedConnection;
//using WindwardReportsDrivers.net.windward.datasource;
//using WindwardReportsDrivers.net.windward.datasource.ado;
//using WindwardReportsDrivers.net.windward.datasource.xml;
//using Exception = System.Exception;

namespace OdataAdvanceConnectionSample
{



    public class OdataAdvancedConnection : IAdvancedConnection
    {

        /// <summary>
        /// When the AutoTag datasource connection window is open, it will query this to see if the datasource type is supported, and if it is, the advance button will appear
        /// Similar to JSON advance connection sample but checks for OData type
        /// </summary>
        public AdvancedConnectionUtils.DATASOURCE_TYPE[] TypesSupported { get { return new AdvancedConnectionUtils.DATASOURCE_TYPE[] { AdvancedConnectionUtils.DATASOURCE_TYPE.ODATA }; } }

        /// <summary>
        /// This is the title for the advanced properties window
        /// </summary>
        public string Title { get { return "FPX Advanced OData Connection"; } }


        /// <summary>
        /// It will read this Dictionary to create the advanced window, when the datasource connection is being created.
        /// </summary>
        /// <param name="type">This is the data type we need the params for (this one will be OData)</param>
        /// <returns>The parameters that need to be set are returned</returns>
        public IDictionary<string, string> GetProperties(AdvancedConnectionUtils.DATASOURCE_TYPE type)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            properties["Odata Download URI"] = "";
            properties["Username"] = "";
            properties["Password"] = "";

            return properties;
        }


        /// <summary>
        /// Converts a uri to what we need to connect with. Returns a URL & props or a stream
        /// </summary>
        /// <param name="type">This is the data type we need need the params for (this one will be OData)</param>
        /// <param name="url">This is the URL that AutoTag constructed</param>
        /// <param name="properties">The properties to be used with the URL (dictionary)</param>
        /// <returns>The adjusted URL to use</returns>
        public AdvancedConnectionUrl GetUrl(AdvancedConnectionUtils.DATASOURCE_TYPE type, string url, IDictionary<string, string> properties)
        {
            Uri loginUri = new Uri(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(loginUri);
            string body = "{\"username\" : \"" + properties["Username"] + "\", \"password\" : \"" + properties["Password"] + "\"}";
            byte[] toSend = Encoding.ASCII.GetBytes(body);
            request.Method = "POST";
            request.ContentType = "application/odata";
            request.ContentLength = body.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(toSend, 0, toSend.Length);
            }

            // Get the OData ID
            string ODataId = null;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                ODataResponse ODATA = JsonConvert.DeserializeObject<ODataResponse>(responseString);
                if (!ODATA.success)
                    throw new CannotAuthorizeException("Not able to log into the Odata data source. Error: " + ODATA.error);

                ODataId = response.Headers["Set-Cookie"].Remove(response.Headers["Set-Cookie"].IndexOf(";")).Substring(11);
            }




            // Download the JSON file
            request = (HttpWebRequest)WebRequest.Create(properties["OData Download URL"]);
            request.Method = "GET";
            request.CookieContainer = new CookieContainer(1);
            request.CookieContainer.Add(new Cookie("ODATAID", ODataId) { Domain = loginUri.Host });

            // new memory stream created for read information
            MemoryStream odata = new MemoryStream();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    // buffer allocated for response
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        odata.Write(buffer, 0, bytesRead);
                    }
                }

                odata.Seek(0, SeekOrigin.Begin);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                    throw new CannotAuthorizeException("Unable to read the odata data. Error: " + new StreamReader(odata).ReadToEnd());

            }

            AdvancedConnectionUrl toRet = new AdvancedConnectionUrl(null, properties, odata);
            return toRet;
        }
        private class ODataResponse
        {
            public string rfst;
            public string userId;
            public bool success;
            public string error;
        }

        private class CannotAuthorizeException : Exception
        {
            public CannotAuthorizeException(string message)
                : base(message)
            {
            }
        }

    }
}