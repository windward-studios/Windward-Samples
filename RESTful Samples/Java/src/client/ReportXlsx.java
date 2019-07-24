package client;

import java.io.InputStream;
import java.io.OutputStream;
import java.net.URL;

/**
 * Created by Bassem on 4/12/2015.
 */
public class ReportXlsx extends Report {
    public ReportXlsx(URL baseUrl, InputStream template, OutputStream report) {
        super(baseUrl, template, report);
    }

    public ReportXlsx(URL baseUrl, InputStream template) {
        super(baseUrl, template);
    }

    @Override
    public String outputFormat() {
        return "xlsx";
    }
}
