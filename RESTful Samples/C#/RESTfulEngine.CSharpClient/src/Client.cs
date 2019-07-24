using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace RESTfulEngine.CSharpClient
{
    class Client
    {
        public static HttpStatusCode Post(Uri uri, XDocument body, int timeout, out XDocument result)
        {
            var request = CreatetPostRequest(uri, timeout);
            SetRequestBody(request, body);
            return ProcessRequest(request, out result);
        }

        public static HttpStatusCode Get(Uri uri, int timeout, out XDocument result)
        {
            var request = CreatetGetRequest(uri, timeout);
            return ProcessRequest(request, out result);
        }

        public static HttpStatusCode Delete(Uri uri, int timeout, out XDocument result)
        {
            var request = CreatetDeleteRequest(uri, timeout);
            return ProcessRequest(request, out result);
        }

        private static HttpStatusCode ProcessRequest(HttpWebRequest request, out XDocument result)
        {
            using (var response = SendRequest(request))
            {
                result = GetResponseBody(response);

                return response.StatusCode;
            }
        }

        private static HttpWebResponse SendRequest(HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                    return (HttpWebResponse)e.Response;
                else
                    throw;
            }
        }

        private static void SetRequestBody(HttpWebRequest request, XDocument body)
        {
            var encoding = new UTF8Encoding(false);
            var bytes = encoding.GetBytes(body.ToString());
            request.ContentLength = bytes.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private static XDocument GetResponseBody(HttpWebResponse response)
        {
            var body = new XDocument();

            if (response.ContentLength > 0)
            {
                using (var stream = response.GetResponseStream())
                {
					try
					{
	                    body = XDocument.Load(XmlReader.Create(stream));
					}
					catch (XmlException)
					{
						// The body contains something other than XML. This normally happens when the server
						// returns an error.
						stream.Position = 0;
						using (var reader = new StreamReader(stream, Encoding.UTF8))
						{
							body.Add(new XElement("Error", reader.ReadToEnd()));
						}
					}
                }
            }
            return body;
        }

        private static HttpWebRequest CreatetPostRequest(Uri uri, int timeout)
        {
            return CreatetRequest(uri, "POST", timeout);
        }

        private static HttpWebRequest CreatetGetRequest(Uri uri, int timeout)
        {
            return CreatetRequest(uri, "GET", timeout);
        }

        private static HttpWebRequest CreatetDeleteRequest(Uri uri, int timeout)
        {
            return CreatetRequest(uri, "DELETE", timeout);
        }

        private static HttpWebRequest CreatetRequest(Uri uri, string method, int timeout)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Timeout = timeout == 0 ? System.Threading.Timeout.Infinite : timeout;
            request.Accept = "application/xml";
            request.ContentType = "application/xml";
            return request;
        }
    }
}
