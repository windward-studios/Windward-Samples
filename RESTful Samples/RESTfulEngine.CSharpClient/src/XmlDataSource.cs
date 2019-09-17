/*
 * Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using System;
using System.IO;
using System.Xml.Linq;

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// XML data source provider.
    /// </summary>
    public class XmlDataSource : DataSource
    {
        private string connectionString;
        private string schemaConnectionString;

        private Stream data;
        private Uri uri;

        private Stream schemaData;
        private Uri schemaUri;

        public XmlDataSource(string connectionString)
        {
            this.connectionString = connectionString;
            IsLegacy = true;
        }

        public XmlDataSource(string connectionString, string schemaConnectionString)
            : this(connectionString)
        {
            this.schemaConnectionString = schemaConnectionString;
        }

        /// <summary>
        /// Initializes a new instance of an XML data source.
        /// </summary>
        /// <param name="data">A stream to read the XML data from.</param>
        public XmlDataSource(Stream data)
        {
            this.data = data;
            IsLegacy = true;
        }

        /// <summary>
        /// Initializes a new instance of an XML data source.
        /// </summary>
        /// <param name="uri">An URI to get the XML data from.</param>
        public XmlDataSource(Uri uri)
        {
            this.uri = uri;
            IsLegacy = true;
        }

        public void SetSchema(Stream data)
        {
            schemaData = data;
        }

        public void SetSchema(Uri uri)
        {
            this.schemaUri = uri;
        }

        /// <summary>
        /// Get or set a flag indicating what version of XML implementation
        /// should be used, newer xpath 2.0 or the legacy xpath 1.0.
        /// true by default which makes use of xpath 1.0 to not hurt the
	/// existing clients.
        /// </summary>
        public bool IsLegacy { get; set; }

        internal override XElement GetXml(string name)
        {
            var element = new XElement("Datasource",
                new XElement("Name", name),
                new XElement("Type", (IsLegacy ? "xml" : "xml2"))
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

            if (schemaConnectionString != null)
            {
                element.Add(new XElement("SchemaConnectionString", schemaConnectionString));
            }
            else if (schemaData != null)
            {
                element.Add(new XElement("SchemaData", System.Convert.ToBase64String(Utils.ReadAllBytes(schemaData))));
            }
            else if (schemaUri != null)
            {
                element.Add(new XElement("SchemaUri", uri));
            }

            if (Variables != null && Variables.Count > 0)
            {
                element.Add(GetVariablesXml());
            }
            return element;
        }
    }
}
