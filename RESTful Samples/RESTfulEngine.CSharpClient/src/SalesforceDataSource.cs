using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RESTfulEngine.CSharpClient
{
    public class SalesforceDataSource : DataSource
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _securityToken;

        public SalesforceDataSource(string username, string password, string securityToken)
        {
            _username = username;
            _password = password;
            _securityToken = securityToken;
        }

        internal override XElement GetXml(string name)
        {
            var element = new XElement("Datasource",
                new XElement("Name", name),
                new XElement("Type", "salesforce")
                );
            
            if (_username != null)
                element.Add(new XElement("Username", _username));
            if (_password != null)
                element.Add(new XElement("Password", _password));
            if (_securityToken != null)
                element.Add(new XElement("SalesforceToken", _securityToken));
            
            if (Variables != null && Variables.Count > 0)
            {
                element.Add(GetVariablesXml());
            }
            return element;
        }
    }
}
