import net.windward.env.SystemWrapper;
import net.windward.xmlreport.ProcessReport;

import java.io.File;

/**
 * Sample application to show how to limits calls to Windward to the license thread limit.
 */
public class SemaphoreSample {

    /**
     * Run the example.
     * @param args Run the example.
     */
    public static void main(String[] args) throws Exception {

        // initialize the engine
        ProcessReport.init();

        String path = new File("files").getAbsolutePath();
        String template = new File(path, "Windward Trucking 2 - Template.docx").getAbsolutePath();
        String xmlData = new File(path, "Windward Trucking 2 - Data.xml").getAbsolutePath();
        File reportFolder = new File("out");

        // set up the reports I want to run
        MyRunReport [] myReports = new MyRunReport[10];
        for (int ind = 0; ind < myReports.length; ind++)
            myReports[ind] = new MyRunReport(template, xmlData, new File(reportFolder, "Report_" + ind + ".docx").getAbsolutePath());

        // create a thread for each request
        Thread[] myThreads = new Thread[myReports.length];
        for (int ind = 0; ind < myReports.length; ind++ )
            myThreads[ind] = new Thread(myReports[ind]);

        // start the threads
        for (int ind = 0; ind < myThreads.length; ind++ )
            myThreads[ind].start();

        // wait for them to end
        for (int ind = 0; ind < myThreads.length; ind++ )
            myThreads[ind].join();

        System.out.println("all threads completed");
        SystemWrapper.LaunchFile(reportFolder.getAbsolutePath());
    }
}
