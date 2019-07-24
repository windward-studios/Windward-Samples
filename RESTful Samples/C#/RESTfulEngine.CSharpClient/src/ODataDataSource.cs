using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RESTfulEngine.CSharpClient
{
    public enum ODataProtocol
    {
        NotProvided,
        Identity,
        Basic,
        Credentials,
        WindowsAuth
    }

    public class ODataDataSource : DataSource
    {
        private Uri uri;
        private string domain;
        private string username;
        private string password;
        private int version;

        private ODataProtocol protocol = ODataProtocol.NotProvided;

        public ODataDataSource(Uri uri, int version)
        {
            this.uri = uri;
            this.version = version;
        }

        public ODataDataSource(Uri uri, string username, string password, ODataProtocol protocol, int version)
        {
            this.uri = uri;
            this.username = username;
            this.password = password;
            this.protocol = protocol;
            this.version = version;
        }

        public ODataDataSource(Uri uri, string domain, string username, string password, ODataProtocol protocol, int version)
        {
            this.uri = uri;
            this.domain = domain;
            this.username = username;
            this.password = password;
            this.protocol = protocol;
            this.version = version;
        }

        internal override XElement GetXml(string name)
        {
            var element = new XElement("Datasource",
                new XElement("Name", name),
                new XElement("Type", "odata")
                );

            if (uri != null)
                element.Add(new XElement("Uri", uri));

            if (domain != null)
                element.Add(new XElement("Domain", domain));
            if (username != null)
                element.Add(new XElement("Username", username));
            if (password != null)
                element.Add(new XElement("Password", password));

            switch (protocol)
            {
                case ODataProtocol.Identity:
                    element.Add(new XElement("ODataProtocol", "identity"));
                    break;
                case ODataProtocol.Basic:
                    element.Add(new XElement("ODataProtocol", "basic"));
                    break;
                case ODataProtocol.Credentials:
                    element.Add(new XElement("ODataProtocol", "credentials"));
                    break;
                case ODataProtocol.WindowsAuth:
                    element.Add(new XElement("ODataProtocol", "windowsauth"));
                    break;
            }
            element.Add(new XElement("ODataVersion", version.ToString()));

            if (Variables != null && Variables.Count > 0)
            {
                element.Add(GetVariablesXml());
            }
            return element;
        }
    }
}
