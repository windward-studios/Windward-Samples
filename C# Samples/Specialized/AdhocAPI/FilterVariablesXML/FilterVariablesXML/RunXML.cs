/*
* Copyright (c) 2011 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

using System;
using System.Collections.Generic;
using System.IO;
using net.windward.api.csharp;
using WindwardInterfaces.net.windward.datasource;

namespace FilterVariablesXML
{
	class RunXML
	{
		static void Main(string[] args)
		{
			string templateFilename = Path.GetFullPath("../../Filter_Variables_Southwind_XML.docx");
			Console.Out.WriteLine("Starting FilterVariablesSQL example");
			Console.Out.WriteLine(string.Format("Template: {0}", templateFilename));

			// Initialize the reporting engine. This will throw an exception if the engine is not fully installed or you
			// do not have a valid license key in RunReport.exe.config.
			Report.Init();

			// The lists are arrays of object (and can be List<object>) rather than an arry of the type passed. This is a C# restriction
			// where you cannot pass List<string> for List<object>. You can pass string[] for object[] but for any other collection
			// mechanism you cannot pass a collection of a sub-class. 
			//
			// While type checking will allow any object, Windward will throw an exception of the objects are any type other than
			// string, number (int, float, etc), or DateTime.

			// run with a list for each.
			// It will replace the condition in the forEach select.
			Dictionary<string, object> adHocVariables = new Dictionary<string, object>();
			adHocVariables.Add("employeeId", new FilterList(FilterBase.SORT_ORDER.NO_OVERRIDE, "employeeId", new object[] { 1, 2 }));
			adHocVariables.Add("employeeName", new FilterList(FilterBase.SORT_ORDER.NO_OVERRIDE, "employeeName", new object[] { "Nancy", "Janet" }));
			// We can do this for ${id} even though it is not a select variable. All that matters is the 'EmployeeID = ${id}' in the select
			adHocVariables.Add("id", new FilterList(FilterBase.SORT_ORDER.NO_OVERRIDE, "id", new object[] { 3, 5 }));

			// This is the global ad-hoc variable. There is no ${TerritoryID} in any select.
			FilterList filterList = new FilterList(FilterBase.SORT_ORDER.NO_OVERRIDE, "TerritoryID", new object[] { "02139", "02184" });
			// This is what makes the global ad-hoc variable work. It must be the full XPath to the node we are adding as a filter.
			filterList.setGlobal("/windward-studios/Territories/Territory/@TerritoryID");
			adHocVariables.Add("TerritoryID", filterList);

			RunReport(templateFilename, Path.GetFullPath("../../Filter_Lists_XML.docx"), adHocVariables);


			// run with a condition for each.
			// It will replace the condition in the forEach select.
			// The name passed to FilterCondition.Condition() is the node name (@name for an attribute), not the variable name.
			adHocVariables.Clear();
			adHocVariables.Add("employeeId", new FilterCondition(FilterBase.SORT_ORDER.NO_OVERRIDE, "employeeId",
						new FilterCondition.Condition[] { new FilterCondition.Condition("@EmployeeID", FilterCondition.Condition.OPERATION.EQUALS, 1) },
						true));
			adHocVariables.Add("employeeName", new FilterCondition(FilterBase.SORT_ORDER.NO_OVERRIDE, "employeeName",
						new FilterCondition.Condition[] { new FilterCondition.Condition("FirstName", FilterCondition.Condition.OPERATION.NOT_BEGIN_WITH, "An") },
						true));
			// this is an OR, not an AND
			adHocVariables.Add("id", new FilterCondition(FilterBase.SORT_ORDER.NO_OVERRIDE, "id",
						new FilterCondition.Condition[] { new FilterCondition.Condition("@EmployeeID", FilterCondition.Condition.OPERATION.LESS_THAN_OR_EQUAL, 3),
						new FilterCondition.Condition("EmployeeID", FilterCondition.Condition.OPERATION.EQUALS, 6) },
						false));
			FilterCondition filterCondition = new FilterCondition(FilterBase.SORT_ORDER.NO_OVERRIDE, "TerritoryDescription",
						new FilterCondition.Condition[] { new FilterCondition.Condition("TerritoryDescription", FilterCondition.Condition.OPERATION.BEGIN_WITH, "B") },
						true);
			filterCondition.setGlobal("/windward-studios/Territories/Territory/TerritoryDescription");
			adHocVariables.Add("TerritoryDescription", filterCondition);

			RunReport(templateFilename, Path.GetFullPath("../../Filter_Conditions_XML.docx"), adHocVariables);


			// run with a set value for each (ie, what we've always supported for variables). 
			// For this case it will use the condition in the forEach select.
			adHocVariables.Clear();
			adHocVariables.Add("employeeId", 3);
			adHocVariables.Add("employeeName", "Nancy");
			adHocVariables.Add("id", 5);
			// there is no literal setting allowed for an ad-hoc filter so we'll do an or condition
			filterCondition = new FilterCondition(FilterBase.SORT_ORDER.NO_OVERRIDE, "TerritoryDescription",
				new FilterCondition.Condition[] { new FilterCondition.Condition("TerritoryDescription", FilterCondition.Condition.OPERATION.BEGIN_WITH, "A"),
				new FilterCondition.Condition("TerritoryDescription", FilterCondition.Condition.OPERATION.BEGIN_WITH, "B")},
				false);
			filterCondition.setGlobal("/windward-studios/Territories/Territory/TerritoryDescription");
			adHocVariables.Add("TerritoryDescription", filterCondition);

			RunReport(templateFilename, Path.GetFullPath("../../Filter_Literals_XML.docx"), adHocVariables);

		}

		private static void RunReport(string templateFilename, string outputFilename, Dictionary<string, object> adHocVariables)
		{
			// get the report ready to run.
			using (Stream template = new FileStream(templateFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (Stream output = new FileStream(outputFilename, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					using (Report report = new ReportDocx(template, output))
					{
						report.ProcessSetup();

						// Create the datasource - SouthWind.xml. 
						// We need the XSD too so it knows the type of nodes.
						using (Stream xmlStream = new FileStream(Path.GetFullPath("../../SouthWind.xml"), FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							using (Stream xsdStream = new FileStream(Path.GetFullPath("../../SouthWind.xsd"), FileMode.Open, FileAccess.Read, FileShare.Read))
							{
								using (XmlDataSourceImpl datasource = new XmlDataSourceImpl(xmlStream, xsdStream, true))
								{

									// set the variables to provide list results for each var
									report.Parameters = adHocVariables;

									// run the datasource
									report.ProcessData(datasource, "");
								}
							}
						}

						// and we're done!
						report.ProcessComplete();
						output.Close();
					}
				}
			}
			Console.Out.WriteLine(string.Format("Completed: {0}", outputFilename));
		}
	}
}
