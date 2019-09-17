/*
 * Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using System.Collections.Generic;
using System.Xml.Linq;

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// The base class for all data source providers.
    /// </summary>
    public abstract class DataSource
    {
        public IList<TemplateVariable> Variables { get; set; }

        protected XElement GetVariablesXml()
        {
            var element = new XElement("Variables");
            foreach (TemplateVariable variable in Variables)
            {
                element.Add(new XElement("Variable",
                    new XElement("Name", variable.Name),
                    new XElement("Value", variable.Value)
                    ));
            }
            return element;
        }

        internal abstract XElement GetXml(string name);
    }
}
