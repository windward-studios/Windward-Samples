/*
 * Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using System.Xml.Linq;

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// ADO data source provider.
    /// </summary>
    public class AdoDataSource : DataSource
    {
        private string className;
        private string connectionString;

        public AdoDataSource(string className, string connectionString)
        {
            this.className = className;
            this.connectionString = connectionString;
        }

        internal override XElement GetXml(string name)
        {
            var element = new XElement("Datasource",
                new XElement("Name", name),
                new XElement("Type", "sql"),
                new XElement("ClassName", className),
                new XElement("ConnectionString", connectionString)
                );
            if (Variables != null && Variables.Count > 0)
            {
                element.Add(GetVariablesXml());
            }
            return element;
        }
    }
}
