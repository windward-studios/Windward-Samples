using System;
using System.IO;
using System.Threading;
using net.windward.api.csharp;

namespace SemaphoreSample
{
	/// <summary>
	/// Sample application to show how to limits calls to Windward to the license thread limit.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Run the example.
		/// </summary>
		/// <param name="args">Run the example.</param>
		public static void Main(string[] args)
		{

			// Initialize the engine
			Report.Init();

			string path = Path.GetFullPath("..\\..\\files");
			string template = Path.GetFullPath(Path.Combine(path, "Windward Trucking 2 - Template.docx"));
			string xmlData = Path.GetFullPath(Path.Combine(path, "Windward Trucking 2 - Data.xml"));

            // put reports in out directory
            string pathReports = Path.GetFullPath("..\\..\\out");
            if (! Directory.Exists(pathReports))
                Directory.CreateDirectory(pathReports);

			// set up the reports I want to run
			MyRunReport [] myReports = new MyRunReport[10];
			for (int ind = 0; ind < myReports.Length; ind++)
                myReports[ind] = new MyRunReport(template, xmlData, Path.Combine(pathReports, string.Format("Report_{0}.docx", ind)));

			// create a thread for each request
			Thread[] myThreads = new Thread[myReports.Length];
			for (int ind = 0; ind < myReports.Length; ind++ )
				myThreads[ind] = new Thread(myReports[ind].RunReport);

			// start the threads
			foreach (Thread threadOn in myThreads)
				threadOn.Start();

			// wait for them to end
			foreach (Thread threadOn in myThreads)
				threadOn.Join();

			Console.Out.WriteLine("all threads completed");
			System.Diagnostics.Process.Start(pathReports);
		}
	}
}
