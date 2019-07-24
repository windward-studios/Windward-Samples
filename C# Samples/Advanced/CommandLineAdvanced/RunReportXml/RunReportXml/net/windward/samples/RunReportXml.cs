/*
* Copyright (c) 2011 by Windward Studios, Inc. All rights reserved.
*
* This program can be copied or used in any manner desired.
*/

using System.Collections.Generic;
using log4net.Config;
using net.windward.api.csharp;
using System;
using System.IO;
using System.Diagnostics;

namespace RunReportXml.net.windward.samples
{

	/// <summary>
	/// A sample usage of Windward Reports. This is a very simple implementation where it will always uses a single XML datasource and
	/// always generate a PDF report. It does not support schemas or datasets. Please look at RunReport for that functionality. The XML
	/// file must be a filename (ie not http, etc).
	/// 
	/// To get the parameters, the RunReport with no parameters and it will list them all out.
	/// </summary>
	class RunReportXml
	{
		/// <summary>
		/// Create a report using Windward Reports.
		/// </summary>
		/// <param name="args">run with no parameters to list out usage.</param>
		static void Main(string[] args)
		{
            //
            // added this code to allow the example to run with a default set of parameters and return a result
            //
            //hard-code the arguments for the sample templates that ship with the example
            CommandLine cmdLine;

            if (args.Length < 2)
            {
              string[] exargs = { "InternetMarketingReport.docx", "testxmlreport.pdf", "-xml:INTMARKETING", "InternetMarketingData.xml"};
              // if no arguments, then we list out the usage.
              Console.WriteLine("\n\n\nPress any key to display the usage parameters...");
              Console.ReadKey();
              DisplayUsage();
              Console.WriteLine("\n\n\nPress any key to run the example with default parameters...");
              Console.ReadKey();
              cmdLine = CommandLine.Factory(exargs);
            }
            else
              cmdLine = CommandLine.Factory(args);
            //
            // Uncomment the following code to allow processing of the commandline parameters (other than the defaults)
            //and change all references from "exargs" to "args"

            // if no arguments, then we list out the usage.
            //if (args.Length < 2)
			//	{
			//		DisplayUsage();
            //        return;
            //   }

			// parse the arguments passed in. This method makes no calls to Windward, it merely organizes the passed in arguments.

			// the try here is so we can print out an exception if it is thrown. This code does minimal error checking and no other
			// exception handling to keep it simple & clear.
			try
			{
				// This turns on log4net logging. You can also call log4net.Config.XmlConfigurator.Configure(); directly.
				// If you do not make this call, then there will be no logging (which you may want off).
				BasicConfigurator.Configure();

				// Initialize the reporting engine. This will throw an exception if the engine is not fully installed or you
				// do not have a valid license key in RunReportXml.exe.config.
				Report.Init();

				// get the template and output file streams. 
				using (Stream template = new FileStream(cmdLine.TemplateFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (Stream output = new FileStream(cmdLine.ReportFilename, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						Console.Out.WriteLine(string.Format("Template: {0}", cmdLine.TemplateFilename));
						Console.Out.WriteLine(string.Format("Report: {0}", cmdLine.ReportFilename));

						// Create the report object, based on the file extension
						using (Report report = new ReportPdf(template, output))
						{

							// This first call parses the template and prepares the report so we can apply data to it.
							report.ProcessSetup();

							// If we have a datasource, we set it up.
							if (! string.IsNullOrEmpty(cmdLine.DatasourceFilename))
							{
								Console.Out.WriteLine(string.Format("XML datasource: {0}", cmdLine.DatasourceFilename));
								using (Stream dsStream = new FileStream(cmdLine.DatasourceFilename, FileMode.Open, FileAccess.Read))
								{
									using (XmlDataSourceImpl datasource = new XmlDataSourceImpl(dsStream, false))
									{
										// Assign any passed variables.
										report.Parameters = cmdLine.Map;

										// this applies the datasource to the report populating the tags.
										report.ProcessData(datasource, cmdLine.DatasourceName);
									}
								}
							}

							// Now that all the data has been applied, we generate the final output report. This does the
							// page layout and then writes out the output file.
							report.ProcessComplete();

							Console.Out.WriteLine(string.Format("{0} built, {1} pages long", cmdLine.ReportFilename, report.NumPages));
                            // need a launcher for the result
                            Process.Start(cmdLine.ReportFilename);
						}
					}
				}
			}
			catch (Exception ex)
			{
				while (ex != null)
				{
					Console.Error.WriteLine(string.Format("Error: {0}\n     stack: {1}\n", ex.Message, ex.StackTrace));
					ex = ex.InnerException;
				}
				throw;
			}
		}

		private static void DisplayUsage()
		{
			Console.Out.WriteLine("usage: RunReport template_file report.pdf [-xml xml_file] [key=value | ...]");
			Console.Out.WriteLine("       The template file can be a rtf, xml (WordML), docx, ppts, or xlsx file.");
			Console.Out.WriteLine("       -xml filename - passes an xml file as the datasource");
			Console.Out.WriteLine("           if a datasource is named you use the syntax -type:name (ex: -xml:name filename.xml)");
			Console.Out.WriteLine("       You can have 0-N key=value pairs that are passed to the datasource Map property");
			Console.Out.WriteLine("           If the value starts with I', F', or D' it parses it as an integer, float, or date(yyyy-MM-ddThh:mm:ss)");
		}
	}

	/// <summary>
	/// This class contains everything passed in the command line. It makes no calls to Windward Reports.
	/// </summary>
	internal class CommandLine
	{
		private readonly string templateFilename;
		private readonly string reportFilename;
		private readonly Dictionary<string, object> map;
		private string datasourceName;
		private string datasourceFilename;

		/// <summary>
		/// Create the object.
		/// </summary>
		/// <param name="templateFilename">The name of the template file.</param>
		/// <param name="reportFilename">The name of the report file. null for printer reports.</param>
		public CommandLine(string templateFilename, string reportFilename)
		{
			this.templateFilename = Path.GetFullPath(templateFilename);
			if (!reportFilename.ToLower().EndsWith(".prn"))
				reportFilename = Path.GetFullPath(reportFilename);
			this.reportFilename = reportFilename;
			map = new Dictionary<string, object>();
		}

		/// <summary>
		/// The name of the template file.
		/// </summary>
		public string TemplateFilename
		{
			get { return templateFilename; }
		}

		/// <summary>
		/// The name of the report file. null for printer reports.
		/// </summary>
		public string ReportFilename
		{
			get { return reportFilename; }
		}

		/// <summary>
		/// The name/value pairs for variables passed to the datasources.
		/// </summary>
		public Dictionary<string, object> Map
		{
			get { return map; }
		}

		/// <summary>
		/// The datasource name. Can be null for an unnamed datasource.
		/// </summary>
		public string DatasourceName
		{
			get { return datasourceName; }
			set { datasourceName = value; }
		}

		/// <summary>
		/// The datasource filename. Null if no datasource.
		/// </summary>
		public string DatasourceFilename
		{
			get { return datasourceFilename; }
			set { datasourceFilename = value; }
		}

		/// <summary>
		/// Create a CommandLine object from the command line passed to the program.
		/// </summary>
		/// <param name="args">The arguments passed to the program.</param>
		/// <returns>A CommandLine object populated from the args.</returns>
		public static CommandLine Factory(IList<string> args)
		{

			CommandLine rtn = new CommandLine(args[0], args[1]);

			for (int ind = 2; ind < args.Count; ind++)
			{
				string[] sa = args[ind].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				string cmd = sa[0].Trim();
				string name = sa.Length < 2 ? "" : sa[1].Trim();

				if (cmd == "-xml")
				{
					string filename = args[++ind];
					rtn.DatasourceName = name;
					rtn.DatasourceFilename = filename;
					continue;
				}

				// assume this is a key=value
				string[] keyValue = args[ind].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				if (keyValue.Length != 2)
					throw new ArgumentException(string.Format("Unknown option {0}", args[ind]));
				object value;
				if (keyValue[1].StartsWith("I'"))
					value = Convert.ToInt64(keyValue[1].Substring(2));
				else if (keyValue[1].StartsWith("F'"))
					value = Convert.ToDouble(keyValue[1].Substring(2));
				else if (keyValue[1].StartsWith("D'"))
					value = Convert.ToDateTime(keyValue[1].Substring(2));
				else
					value = keyValue[1];
				rtn.map.Add(keyValue[0], value);
			}

			return rtn;
		}
	}
}
