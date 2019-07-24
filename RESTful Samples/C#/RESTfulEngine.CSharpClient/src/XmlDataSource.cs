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
        private Stream data;
        private Uri uri;

        private Stream schemaData;
        private Uri schemaUri;

        /// <summary>
        /// Initializes a new instance of an XML data source.
        /// </summary>
        /// <param name="data">A stream to read the XML data from.</param>
        public XmlDataSource(Stream data)
        {
            this.data = data;
        }

        /// <summary>
        /// Initializes a new instance of an XML data source.
        /// </summary>
        /// <param name="uri">An URI to get the XML data from.</param>
        public XmlDataSource(Uri uri)
        {
            this.uri = uri;
        }

        public void SetSchema(Stream data)
        {
            schemaData = data;
        }

        public void SetSchema(Uri uri)
        {
            this.schemaUri = uri;
        }

        internal override XElement GetXml(string name)
        {
            var element = new XElement("Datasource",
                new XElement("Name", name),
                new XElement("Type", "xml")
                );

            if (data != null)
            {
                var bytes = Utils.ReadAllBytes(data);
                element.Add(new XElement("Data", System.Convert.ToBase64String(bytes)));
            }
            else if (uri != null)
                element.Add(new XElement("Uri", uri));

            if (schemaData != null)
                element.Add(new XElement("SchemaData", System.Convert.ToBase64String(Utils.ReadAllBytes(schemaData))));
            else if (schemaUri != null)
                element.Add(new XElement("SchemaUri", uri));

            if (Variables != null && Variables.Count > 0)
            {
                element.Add(GetVariablesXml());
            }
            return element;
        }
    }
}
