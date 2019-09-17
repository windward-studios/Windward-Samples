using System;
using System.IO;
using System.Xml.Linq;

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// JSON data source provider.
    /// </summary>
    public class JsonDataSource : DataSource
    {
        private string connectionString;
        private Stream data;
        private Uri uri;
        private string username;
        private string password;
        private string domain;

        public JsonDataSource(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of a JSON data source.
        /// </summary>
        /// <param name="uri">A stream to read the JSON data from.</param>
        public JsonDataSource(Stream data)
        {
            this.data = data;
        }

        /// <summary>
        /// Initializes a new instance of a JSON data source.
        /// </summary>
        /// <param name="uri">An URI to get the JSON data from.</param>
        public JsonDataSource(Uri uri)
        {
            this.uri = uri;
        }

        /// <summary>
        /// Initializes a new instance of a JSON data source.
        /// </summary>
        /// <param name="uri">An URI to get the JSON data from.</param>
        /// <param name="username">A username.</param>
        /// <param name="password">A password.</param>
        public JsonDataSource(Uri uri, string username, string password)
        {
            this.uri = uri;
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Initializes a new instance of a JSON data source.
        /// </summary>
        /// <param name="uri">An URI to get the JSON data from.</param>
        /// <param name="domain">A domain.</param>
        /// <param name="username">A username.</param>
        /// <param name="password">A password.</param>
        public JsonDataSource(Uri uri, string domain, string username, string password)
        {
            this.uri = uri;
            this.domain = domain;
            this.username = username;
            this.password = password;
        }

        internal override XElement GetXml(string name)
        {
            var element = new XElement("Datasource",
                new XElement("Name", name),
                new XElement("Type", "json")
                );

            if (connectionString != null)
            {
                element.Add(new XElement("ConnectionString", connectionString));
            }
            else if (data != null)
            {
                var bytes = Utils.ReadAllBytes(data);
                element.Add(new XElement("Data", System.Convert.ToBase64String(bytes)));
            }
            else if (uri != null)
            {
                element.Add(new XElement("Uri", uri));
            }

            // Backward compatibility
            if (connectionString == null)
            {
                if (domain != null)
                    element.Add(new XElement("Domain", domain));
                if (username != null)
                    element.Add(new XElement("Username", username));
                if (password != null)
                    element.Add(new XElement("Password", password));
            }

            if (Variables != null && Variables.Count > 0)
            {
                element.Add(GetVariablesXml());
            }

            return element;
        }
    }
}
