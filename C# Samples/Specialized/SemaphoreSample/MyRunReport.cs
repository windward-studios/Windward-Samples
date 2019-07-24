using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Threading;
using WindwardInterfaces.net.windward.api.csharp;
using net.windward.api.csharp;

namespace SemaphoreSample
{
	/// <summary>
	/// Demonstrates how to use a semaphore to limit N threads calling Windward at once.
	/// </summary>
	public class MyRunReport
	{

		private readonly string templateFilename;
		private readonly string xmlDataFilename;
		private readonly string reportFilename;

		private static readonly Semaphore sem;

		/// <summary>
		/// Pulls the number of threads from the config file. If this changes you need to re-start the app.
		/// </summary>
		static MyRunReport()
		{
			NameValueCollection appSettings = ConfigurationManager.AppSettings;
			string strNumThreads = appSettings["NumberThreads"];
			int intNumThreads = 2;
			if (strNumThreads != null)
			{
				int.TryParse(strNumThreads, out intNumThreads);
				intNumThreads = Math.Max(2, intNumThreads);
			}
			sem = new Semaphore(intNumThreads, intNumThreads);
			// if you have multiple apps running, use the following line instead
			// sem = new Semaphore(intNumThreads, intNumThreads, "MySemaphoreName");
		}

		public MyRunReport(string templateFilename, string xmlDataFilename, string reportFilename)
		{
			this.templateFilename = templateFilename;
			this.xmlDataFilename = xmlDataFilename;
			this.reportFilename = reportFilename;
		}

		public void RunReport ()
		{

			Console.Out.WriteLine(string.Format("Requesting report {0}", Path.GetFileName(reportFilename)));
			
			// this will not return until there is an available semaphore. When it returns, the used semaphore count is incremented by one.
			sem.WaitOne();

			try
			{
				Console.Out.WriteLine(string.Format("     processing report {0}", Path.GetFileName(reportFilename)));

				// Open template file and create output file
				using (FileStream template = File.OpenRead(templateFilename))
				{
					using (FileStream output = File.Create(reportFilename))
					{

						// Create report process
						// !!!!! you MUST have using on this to insure it is closed and the thread count for the engine is decreased !!!!!
						using (Report myReport = new ReportDocx(template, output))
						{
							myReport.ProcessSetup();

							// remove this - this is here insure that all threads try to run at once.
							Thread.Sleep(10*1000);

							// Open an inputfilestream for our data file
							FileStream Xml = File.OpenRead(xmlDataFilename);

							// Open a data object to connect to our xml file
							IReportDataSource data = new XmlDataSourceImpl(Xml, false);

							// Close out of our template file and output
							Xml.Close();

							// Run the report process
							myReport.ProcessData(data, "");
							myReport.ProcessComplete();
						}
					}
				}
			}
			finally
			{
				Console.Out.WriteLine(string.Format("          report completed (releasing semaphore) {0}", Path.GetFileName(reportFilename)));

				// you must call this in a finally block so it is always called.
				// this decrements the used semaphore count by one.
				sem.Release(1);
			}
		}
	}
}
