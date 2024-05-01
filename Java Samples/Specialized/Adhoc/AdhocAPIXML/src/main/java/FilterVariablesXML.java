/*
* Copyright (c) 2011 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

import net.windward.datasource.*;
import net.windward.datasource.dom4j.*;
import net.windward.xmlreport.ProcessDocx;
import net.windward.xmlreport.ProcessReport;

import java.io.*;
import java.util.*;

public class FilterVariablesXML {

	public static void main(String[] args) throws Exception {

		String templateFilename = new File("Filter_Variables_Southwind_XML.docx").getAbsolutePath();
		System.out.println("Starting FilterVariables XML example");
		System.out.println("Template: " + templateFilename);

		// Initialize the reporting engine. This will throw an exception if the engine is not fully installed or you
		// do not have a valid license key in RunReport.exe.config.
		ProcessReport.init();

		// The lists are arrays of object (and can be List<object>) rather than an arry of the type passed. This is a C# restriction
		// where you cannot pass List<String> for List<object>. You can pass String[] for object[] but for any other collection
		// mechanism you cannot pass a collection of a sub-class.
		//
		// While type checking will allow any object, Windward will throw an exception of the objects are any type other than
		// String, number (int, float, etc), or DateTime.

		// run with a list for each.
		// It will replace the condition in the forEach select.
		Map<String, Object> adHocVariables = new HashMap<String, Object>();
		List<Serializable> list = new ArrayList<Serializable>();
		list.add(1);
		list.add(2);
		adHocVariables.put("employeeId", new SelectList(SelectBase.SORT_NO_OVERRIDE, "employeeId", list));
		list.clear();
		list.add("Nancy");
		list.add("Janet");
		adHocVariables.put("employeeName", new SelectList(SelectBase.SORT_NO_OVERRIDE, "employeeName", list));
		// We can do this for ${id} even though it is not a select variable. All that matters is the 'EmployeeID = ${id}' in the select
		list.clear();
		list.add(3);
		list.add(5);
		adHocVariables.put("id", new SelectList(SelectBase.SORT_NO_OVERRIDE, "id", list));

		// This is the global ad-hoc variable. There is no ${TerritoryID} in any select.
		list.clear();
		list.add("02139");
		list.add("02184");
		SelectList SelectList = new SelectList(SelectBase.SORT_NO_OVERRIDE, "TerritoryID", list);
		// This is what makes the global ad-hoc variable work. It must be the full XPath to the node we are adding as a filter.
		SelectList.setGlobal("/windward-studios/Territories/Territory/@TerritoryID");
		adHocVariables.put("TerritoryID", SelectList);

		RunReport(templateFilename, new File("Filter_Lists_XML.docx").getAbsolutePath(), adHocVariables);


		// run with a condition for each.
		// It will replace the condition in the forEach select.
		// The name passed to SelectFilter.Filter() is the node name (@name for an attribute), not the variable name.
		adHocVariables.clear();
		adHocVariables.put("employeeId", new SelectFilter(SelectBase.SORT_NO_OVERRIDE, "employeeId",
				new SelectFilter.Filter[]{new SelectFilter.Filter("@EmployeeID", SelectFilter.Filter.OP_EQUALS, 1)},
				true));
		adHocVariables.put("employeeName", new SelectFilter(SelectBase.SORT_NO_OVERRIDE, "employeeName",
				new SelectFilter.Filter[]{new SelectFilter.Filter("FirstName", SelectFilter.Filter.OP_NOT_BEGIN_WITH, "An")},
				true));
		// this is an OR, not an AND
		adHocVariables.put("id", new SelectFilter(SelectBase.SORT_NO_OVERRIDE, "id",
				new SelectFilter.Filter[]{new SelectFilter.Filter("@EmployeeID", SelectFilter.Filter.OP_LESS_THAN_OR_EQUAL, 3),
				new SelectFilter.Filter("@EmployeeID", SelectFilter.Filter.OP_EQUALS, 6)},
				false));
		SelectFilter selectFilter = new SelectFilter(SelectBase.SORT_NO_OVERRIDE, "TerritoryDescription",
				new SelectFilter.Filter[] { new SelectFilter.Filter("TerritoryDescription", SelectFilter.Filter.OP_BEGIN_WITH, "B") },
				true);
		selectFilter.setGlobal("/windward-studios/Territories/Territory/TerritoryDescription");
		adHocVariables.put("TerritoryDescription", selectFilter);

		RunReport(templateFilename, new File("Filter_Conditions_XML.docx").getAbsolutePath(), adHocVariables);


		// run with a set value for each (ie, what we've always supported for variables).
		// For this case it will use the condition in the forEach select.
		adHocVariables.clear();
		adHocVariables.put("employeeId", 3);
		adHocVariables.put("employeeName", "Nancy");
		adHocVariables.put("id", 5);
		// there is no literal setting allowed for an ad-hoc filter so we'll do an or condition
		selectFilter = new SelectFilter(SelectBase.SORT_NO_OVERRIDE, "TerritoryDescription",
				new SelectFilter.Filter[] { new SelectFilter.Filter("TerritoryDescription", SelectFilter.Filter.OP_BEGIN_WITH, "A"),
				new SelectFilter.Filter("TerritoryDescription", SelectFilter.Filter.OP_BEGIN_WITH, "B")},
				false);
		selectFilter.setGlobal("/windward-studios/Territories/Territory/TerritoryDescription");
		adHocVariables.put("TerritoryDescription", selectFilter);

		RunReport(templateFilename, new File("Filter_Literals_XML.docx").getAbsolutePath(), adHocVariables);
	}

	private static void RunReport(String templateFilename, String outputFilename, Map<String, Object> adHocVariables) throws Exception
	{
		// get the report ready to run.
		FileInputStream template = new FileInputStream(templateFilename);
		FileOutputStream output = new FileOutputStream(outputFilename);
		ProcessReport report = new ProcessDocx(template, output);
		report.processSetup();

		// Create the datasource - SouthWind.xml.
		// We need the XSD too so it knows the type of nodes.
		FileInputStream xmlStream = new FileInputStream(new File("SouthWind.xml").getAbsolutePath());
		FileInputStream xsdStream = new FileInputStream(new File("SouthWind.xsd").getAbsolutePath());
		DataSourceProvider datasource = new Dom4jDataSource(xmlStream, xsdStream);

		// set the variables to provide list results for each var
		report.setParameters(adHocVariables);

		// run the datasource
        System.out.println("Generating report...");
		report.processData(datasource, "");
		datasource.close();
		xsdStream.close();
		xmlStream.close();

		// and we're done!
		report.processComplete();
		output.close();
		template.close();
		report.close();
		System.out.println("Completed: " + outputFilename);
	}
}
