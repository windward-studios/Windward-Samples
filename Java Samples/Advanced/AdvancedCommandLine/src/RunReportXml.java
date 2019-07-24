/*
* Copyright (c) 2011 by Windward Studios, Inc. All rights reserved.
*
*
* This program can be copied or used in any manner desired.
*/

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import java.net.URL;
import java.text.SimpleDateFormat;

import java.util.HashMap;
import java.util.Map;

import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.env.MyParsePosition;
import net.windward.format.htm.HtmlImage;
import net.windward.xmlreport.*;

/**
 * A sample usage of Windward Reports. This is a very simple implementation where it will always uses a single XML
 * datasource and always generate a PDF report. It does not support schemas or datasets. Please look at RunReport for
 * that functionality. The XML file must be a filename (ie not http, etc).
 * <p/>
 * To get the parameters, the RunReport with no parameters and it will list them all out.
 */

public class RunReportXml {

	/**
	 * Create a report using Windward Reports.
	 *
	 * @param args Run with no parameters to list out usage.
	 * @throws Throwable Thrown if anything goes wrong.
	 */
	public static void main(String[] args) throws Throwable {

		// if no arguments, then we list out the usage.
		if (args.length < 2) {
			DisplayUsage();
			return;
		}

		// parse the arguments passed in. This method makes no calls to Windward, it merely organizes the passed in arguments.
		CommandLine cmdLine = CommandLine.Factory(args);

		// the try here is so we can print out an exception if it is thrown. This code does minimal error checking and no other
		// exception handling to keep it simple & clear.
		try {

			// Initialize the reporting engine. This will throw an exception if the engine is not fully installed or you
			// do not have a valid license key in RunReport.exe.config.
			ProcessReport.init();

			// get the template and output file streams. Output is null for printers
			InputStream template = openInputStream(cmdLine.getTemplateFilename());
			OutputStream reportOutput = new FileOutputStream(cmdLine.getReportFilename());
			System.out.println("Template: " + cmdLine.getTemplateFilename());
			System.out.println("Report: " + cmdLine.getReportFilename());

			// Create the PDF report object, based on the file extension
			ProcessReportAPI report = new ProcessPdf(template, reportOutput);

			// This first call parses the template and prepares the report so we can apply data to it.
			report.processSetup();

			// If we have a datasource, we set it up.
			if (cmdLine.getDatasourceFilename() != null) {

				System.out.println("XML datasource: " + cmdLine.getDatasourceFilename());
				InputStream dsStream = openInputStream(cmdLine.getDatasourceFilename());
				DataSourceProvider datasource = new Dom4jDataSource(dsStream);

				report.setParameters(cmdLine.getMap());

				// this applies the datasource to the report populating the tags.
				report.processData(datasource, cmdLine.getDatasourceName());

				// close the stream and datasource.
				dsStream.close();
				datasource.close();
			}

			// Now that all the data has been applied, we generate the final output report. This does the
			// page layout and then writes out the output file.
			report.processComplete();

			// Close the output template to get everything written.
			template.close();
			if (reportOutput != null)
				reportOutput.close();

			System.out.println("Report complete, " + report.getNumPages() + " pages long");
			report.close();

		} catch (Throwable t) {
			System.err.println("Error: " + t.getMessage());
			t.printStackTrace();
			throw t;
		}
	}

	private static void DisplayUsage() {
		System.out.println("usage: RunReport template_file report.pdf [-xml xml_file] [key=value | ...]");
		System.out.println("       The template file can be a rtf, xml (WordML), docx, pptx, or xlsx file.");
		System.out.println("       -xml filename - passes an xml file as the datasource");
		System.out.println("           -xml xmlFilename;schemaFilename - passes an xml file and a schema file as the datasource");
		System.out.println("           -xml:name names this datasource with name");
		System.out.println("       You can have 0-N key=value pairs that are passed to the datasource Map property");
		System.out.println("            If the value starts with I', F', or D' it parses it as an integer, float, or date(yyyy-MM-ddThh:mm:ss)");
	}

	private static InputStream openInputStream(String filename) throws IOException {

		int pos = filename.indexOf(':');
		if ((pos != -1) && (pos != 1))
			return new URL(filename).openStream();
		return new FileInputStream(filename);
	}

	/**
	 * This class contains everything passed in the command line. It makes no calls to Windward Reports.
	 */
	private static class CommandLine {
		private String templateFilename;
		private String reportFilename;
		private String name;
		private String filename;
		private Map<String,Object> map;

		/**
		 * Create the object.
		 *
		 * @param templateFilename The name of the template file.
		 * @param reportFilename   The name of the report file. null for printer reports.
		 */
		public CommandLine(String templateFilename, String reportFilename) {
			this.templateFilename = GetFullPath(templateFilename);
			if (!reportFilename.toLowerCase().endsWith(".prn"))
				reportFilename = GetFullPath(reportFilename);
			this.reportFilename = reportFilename;
			map = new HashMap<String,Object>();
		}

		private static String GetFullPath(String filename) {
			int pos = filename.indexOf(':');
			if ((pos == -1) || (pos == 1))
				return new File(filename).getAbsolutePath();
			return filename;
		}

		/**
		 * The name of the template file.
		 *
		 * @return The name of the template file.
		 */
		public String getTemplateFilename() {
			return templateFilename;
		}

		/**
		 * The name of the report file. null for printer reports.
		 *
		 * @return The name of the report file. null for printer reports.
		 */
		public String getReportFilename() {
			return reportFilename;
		}

		/**
		 * The name for the datasource.
		 *
		 * @return The name for the datasource.
		 */
		public String getDatasourceName() {
			return name;
		}

		/**
		 * The XML filename.
		 *
		 * @return The XML filename.
		 */
		public String getDatasourceFilename() {
			return filename;
		}

		/**
		 * The name/value pairs for variables passed to the datasources. key is a String and value is a String,
		 * Number, or Date.
		 *
		 * @return The name/value pairs for variables passed to the datasources.
		 */
		public Map<String, Object> getMap() {
			return map;
		}

		/**
		 * Create a CommandLine object from the command line passed to the program.
		 *
		 * @param args The arguments passed to the program.
		 * @return A CommandLine object populated from the args.
		 */
		public static CommandLine Factory(String[] args) {

			CommandLine rtn = new CommandLine(args[0], args[1]);

			for (int ind = 2; ind < args.length; ind++) {
				int pos = args[ind].indexOf(':');
				String name = pos == -1 ? "" : args[ind].substring(pos + 1);
				String cmd = pos == -1 ? args[ind] : args[ind].substring(0, pos);

				if (cmd.equals("-xml")) {
					rtn.filename = args[++ind];
					rtn.name = name;
					continue;
				}

				// assume this is a key=value
				int equ = args[ind].indexOf('=');
				if (equ == -1)
					throw new IllegalArgumentException("Unknown option " + args[ind]);
				String key = args[ind].substring(0, equ);
				String value = args[ind].substring(equ + 1);

				Object val;
				if (value.startsWith("I'"))
					val = Long.valueOf(value.substring(2));
				else if (value.startsWith("F'"))
					val = Double.valueOf(value.substring(2));
				else if (value.startsWith("D'")) {
					MyParsePosition pp = new MyParsePosition(0);
					SimpleDateFormat stdFmt = new SimpleDateFormat("yyyy-MM-dd'T'hh:mm:ss");
					val = stdFmt.parse(value.substring(2), pp);
				} else
					val = value;
				rtn.map.put(key, val);
			}
			return rtn;
		}
	}
}
