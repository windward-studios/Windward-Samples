import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.xmlreport.*;

import java.io.*;
import java.util.concurrent.Semaphore;

/**
 * Demonstrates how to use a semaphore to limit N threads calling Windward at once.
 */
public class MyRunReport implements Runnable {

	private String templateFilename;
	private String xmlDataFilename;
	private String reportFilename;

	private static Semaphore sem;

	/// <summary>
	/// Pulls the number of threads from the config file. If this changes you need to re-start the app.
	/// </summary>
	static
	{

		String strNumThreads = System.getProperty("NumberThreads");
		int intNumThreads = 2;
		if (strNumThreads != null && strNumThreads.trim().length() > 0)
		{
			intNumThreads = Integer.parseInt(strNumThreads.trim());
			intNumThreads = Math.max(2, intNumThreads);
		}
		sem = new Semaphore(intNumThreads, true);
	}

	public MyRunReport(String templateFilename, String xmlDataFilename, String reportFilename)
	{
		this.templateFilename = templateFilename;
		this.xmlDataFilename = xmlDataFilename;
		this.reportFilename = reportFilename;
	}

	public void run () {

		try {
			String filename =  new File(reportFilename).getName();
			System.out.println("Requesting report " + filename);

			// this will not return until there is an available semaphore. When it returns, the used semaphore count is incremented by one.
			sem.acquire();
			ProcessReportAPI report = null;

			try
			{
				System.out.println("     processing report " + filename);

				// To generate a report, first we need a ProcessReport object.  For now, we're using the
				// pdf format to output.
				FileInputStream template = new FileInputStream(templateFilename);
				FileOutputStream output = new FileOutputStream(reportFilename);
				report = new ProcessDocx(template, output);

				// Preparation...
				report.processSetup();

				// remove this - this is here insure that all threads try to run at once.
				Thread.sleep(10 * 1000);

				// Set up the datasource. The parameters are connector package, url, username, password.
				// For each type of datasource, the connector package is different
				DataSourceProvider datasource = new Dom4jDataSource(new FileInputStream(xmlDataFilename));

				// Finally, send it to Windward for processing.  The second parameter is the name of the
				// datasource.  This should match the name used in your template.
				report.processData(datasource, "");

				// And... DONE!
				report.processComplete();

				//	Clean up my resources
				if(template != null)
					template.close();
				if(output != null)
					output.close();
				

			} catch (Exception e) {
				e.printStackTrace();
			} finally
			{
				System.out.println("          report completed (releasing semaphore) " + filename);

				// Critical to call this before the release. This ends the report holding
				// the engine thread count.
				if (report != null)
					report.close();

				// you must call this in a finally block so it is always called.
				// this decrements the used semaphore count by one.
				sem.release();
			}
		} catch (InterruptedException e) {
			e.printStackTrace();
		}
	}
}
