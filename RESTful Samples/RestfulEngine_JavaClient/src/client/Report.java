package client;

import com.sun.org.apache.xerces.internal.impl.dv.util.Base64;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import javax.xml.bind.DatatypeConverter;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

/**
 * Created by Bassem on 4/12/2015.
 */
public abstract class Report {

    private URL baseUrl;
    private InputStream template;
    private OutputStream report;
    private String guide;

    private Dataset[] datasets;

    public Report(URL baseUrl, InputStream template, OutputStream report)
    {
        ctor(baseUrl);
        this.template = template;
        this.report = report;
    }

    public Report(URL baseUrl, InputStream template)
    {
        ctor(baseUrl);
        this.template = template;
    }

    private void ctor(URL baseUri) {
        String url = baseUri.toString();
        if (!url.endsWith("/"))
            url += "/";
        try {
            this.baseUrl = new URL(url);
        } catch (MalformedURLException e) {
            e.printStackTrace();//TODO
        }
        // Set up default values
        timeout = 0;
        hyphenate = Hyphenation.Template;
        trackImports = false;
        removeUnusedFormats = true;
        copyMetadata = CopyMetadataOption.IfNoDatasource;
    }

    private static String adjustURL(URL baseUri)  {
        String url = baseUri.toString();
        if (!url.endsWith("/"))
            url += "/";
        return url;
    }

    public static Version GetVersion(URL baseUri)throws Exception
    {
        URL uri = new URL(String.format("%1$sv1/version", adjustURL(baseUri)));

        Client.OutputData outputData = Client.get(uri);
        int status = outputData.statusCode;
        Document outputXMl = outputData.outputXMl;
        if (status == HttpURLConnection.HTTP_OK)
        {
            List<String>l1=Utils.getTextValuesByTagName(outputXMl.getDocumentElement(), "ServiceVersion");
            List<String>l2=Utils.getTextValuesByTagName(outputXMl.getDocumentElement(), "EngineVersion");
            if(l1.size()>0&&l2.size()>0)
                return new Version(l1.get(0),l2.get(0));
            else
                throw new ReportException("Version Data not sent.");
        }

        else
            return null;
    }

   public  void process(HashMap<String, DataSource> dataSources) throws Exception
    {
        Document xml = createXmlDocument();
        applyDatasources(xml, dataSources);
        process(xml);
    }

    public void process() throws Exception
    {
        Document xml = createXmlDocument();
        process(xml);
    }

    protected void process(Document doc) throws Exception {
        SetReportOption(doc, "Description", description);
        SetReportOption(doc, "Title", title);
        SetReportOption(doc, "Subject", subject);
        SetReportOption(doc, "Keywords", keywords);
        SetReportOption(doc, "Locale", locale);

        Element timeOutElement = doc.createElement("Timeout");
        timeOutElement.appendChild(doc.createTextNode(timeout+""));
        doc.getDocumentElement().appendChild(timeOutElement);//TODO  root

        Element hyphenateElement = doc.createElement("Hyphenate");
        switch (hyphenate)
        {
            case On:
                hyphenateElement.appendChild(doc.createTextNode("on"));
                break;
            case Off:
                hyphenateElement.appendChild(doc.createTextNode("off"));
                break;
            case Template:
                hyphenateElement.appendChild(doc.createTextNode("template"));
                break;
        }
        doc.getDocumentElement().appendChild(hyphenateElement);//TODO root

        Element trackElement = doc.createElement("TrackImports");
        trackElement.appendChild(doc.createTextNode(trackImports+""));
        doc.getDocumentElement().appendChild(trackElement);//TODO  root

        Element removeElement = doc.createElement("RemoveUnusedFormats");
        removeElement.appendChild(doc.createTextNode(removeUnusedFormats+""));
        doc.getDocumentElement().appendChild(removeElement);//TODO  root

        Element copyMetaElement = doc.createElement("CopyMetadata");
        switch (copyMetadata)
        {
            case IfNoDatasource:
                copyMetaElement.appendChild(doc.createTextNode("nodatasource"));
                break;
            case Never:
                copyMetaElement.appendChild(doc.createTextNode("never"));
                break;
            case Always:
                copyMetaElement.appendChild(doc.createTextNode("always"));
                break;
        }
        doc.getDocumentElement().appendChild(copyMetaElement);//TODO root

        if(report==null){
            Element asyncElement = doc.createElement("Async");
            asyncElement.appendChild(doc.createTextNode(true+""));
            doc.getDocumentElement().appendChild(asyncElement);//TODO root
        }

        if (datasets != null) {
            Element datasetsElement = doc.createElement("Datasets");

            for (Dataset dataset : datasets) {
                datasetsElement.appendChild(dataset.getXml(doc));
            }

            doc.getDocumentElement().appendChild(datasetsElement);
        }

        //TODO here post and get
        Client.OutputData outputData = Client.post(new URL(String.format("%1$sv1/reports", baseUrl)), doc);
        Document outputXMl = outputData.outputXMl;
        int statusCode = outputData.statusCode;
        if(statusCode == HttpURLConnection.HTTP_OK){
            if(report != null)
                readReport(outputXMl);
            else
                readGuide(outputXMl);

        }else{
            throw new ReportException(outputXMl.toString());
        }
    }

    private void readGuide(Document outputXMl) throws Exception{
        List<String> output = Utils.getTextValuesByTagName(outputXMl.getDocumentElement(), "Guid");
        if(output.size()>0)
            guide = output.get(0);
        else
            throw new ReportException("Guide Not Exist");

    }

    private void readReport(Document outputXMl) throws Exception{
        List<String> output = Utils.getTextValuesByTagName(outputXMl.getDocumentElement(), "Data");
        if(output.size()>0){
            byte [] reportBytes = Base64.decode(output.get(0));
            report.write(reportBytes);
        }
        else
            throw new ReportException("Report Data Not Exist");
    }



    private void SetReportOption(Document doc, String name, String option)
    {
        if (option != null) {
            Element optionElement = doc.createElement(name);
            optionElement.appendChild(doc.createTextNode(option));
            doc.getDocumentElement().appendChild(optionElement);
        }
    }

    private void applyDatasources(Document doc, HashMap<String, DataSource> dataSources)throws Exception{
        if (dataSources.size() > 0)
        {
            Element dataSourcesElement = doc.createElement("Datasources");
            Iterator it = dataSources.entrySet().iterator();
            while (it.hasNext()) {
                Map.Entry pair = (Map.Entry)it.next();

                dataSourcesElement.appendChild(((DataSource) pair.getValue()).getXml((String) pair.getKey(), doc));
                it.remove();
            }
            doc.getDocumentElement().appendChild(dataSourcesElement);// TODO ensure it's attached to root </template>
        }
    }



    private Document createXmlDocument() throws Exception {
        byte [] templateBytes = Utils.readAllBytes(template);
        String encodedString = Base64.encode(templateBytes);
        DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
        DocumentBuilder docBuilder = docFactory.newDocumentBuilder();
        Document doc = docBuilder.newDocument();
        Element rootElement = doc.createElement("Template");//TODO check it
        doc.appendChild(rootElement);

        Element data = doc.createElement("Data");
        data.appendChild(doc.createTextNode(encodedString));
        rootElement.appendChild(data);

        Element outputFormat = doc.createElement("OutputFormat");
        outputFormat.appendChild(doc.createTextNode(outputFormat()));
        rootElement.appendChild(outputFormat);

        return doc;
    }

    public void delete()throws Exception
    {
        URL uri = new URL(String.format("%1$sv1/reports/%2$s", baseUrl, guide));
        Client.OutputData outputData = Client.delete(uri);
    }

    public byte[] getReport()throws Exception
    {
        URL uri = new URL(String.format("%1$sv1/reports/%2$s", baseUrl, guide));


        Client.OutputData outputData = Client.get(uri);
        int status = outputData.statusCode;
        Document outputXMl = outputData.outputXMl;
        if (status == HttpURLConnection.HTTP_OK )
        {
            List<String> output = Utils.getTextValuesByTagName(outputXMl.getDocumentElement(), "Data");
            if(output.size()>0){
                byte [] reportBytes = Base64.decode(output.get(0));
                return reportBytes;
            }

        }
        return null;
    }

    public Status getStatus() throws Exception{
        URL uri = new URL(String.format("%1$sv1/reports/%2$s/status", baseUrl, guide));

        Client.OutputData outputData = Client.get(uri);
        int status = outputData.statusCode;
        if (status == HttpURLConnection.HTTP_OK)
        {
            return Status.Ready;
        }
        else if (status == HttpURLConnection.HTTP_ACCEPTED)
        {
            return Status.Working;
        }
        else if (status == HttpURLConnection.HTTP_INTERNAL_ERROR)
        {
            return Status.Error;
        }
        else
            return Status.NotFound;
    }

    public abstract String outputFormat();

    public String description;
    public String title;
    public String subject;
    public String keywords;
    public String locale;
    public int timeout;
    public Hyphenation hyphenate;
    public boolean trackImports;
    public boolean removeUnusedFormats;
    public enum Hyphenation
    {
        On,
        Off,
        Template
    }

    public Hyphenation getHyphenate() {
        return hyphenate;
    }

    public void setHyphenate(Hyphenation hyphenate) {
        this.hyphenate = hyphenate;
    }

    public boolean isTrackImports() {
        return trackImports;
    }

    public void setTrackImports(boolean trackImports) {
        this.trackImports = trackImports;
    }

    public boolean isRemoveUnusedFormats() {
        return removeUnusedFormats;
    }

    public void setRemoveUnusedFormats(boolean removeUnusedFormats) {
        this.removeUnusedFormats = removeUnusedFormats;
    }

     public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public String getTitle() {
        return title;
    }

    public void setTitle(String title) {
        this.title = title;
    }

    public String getSubject() {
        return subject;
    }

    public void setSubject(String subject) {
        this.subject = subject;
    }

    public String getKeywords() {
        return keywords;
    }

    public void setKeywords(String keywords) {
        this.keywords = keywords;
    }

    public String getLocale() {
        return locale;
    }

    public void setLocale(String locale) {
        this.locale = locale;
    }

    public int getTimeout() {
        return timeout;
    }

    public void setTimeout(int timeout) {
        this.timeout = timeout;
    }


    public enum CopyMetadataOption
    {
        IfNoDatasource,
        Never,
        Always
    }

    public CopyMetadataOption copyMetadata;

    public CopyMetadataOption getCopyMetadata() {
        return copyMetadata;
    }

    public void setCopyMetadata(CopyMetadataOption copyMetadata) {
        this.copyMetadata = copyMetadata;
    }

    public enum Status
    {
        Ready,
        Working,
        Error,
        NotFound
    }

    /**
     * Gets the list of datasets that should be used for this report generation.
     *
     * @return The array of Dataset objects.
     */
    public Dataset[] getDatasets() {
        return datasets;
    }

    /**
     * Sets the list of datasets that should be used for this report generation.
     *
     * @param datasets The array of Dataset objects.
     */
    public void setDatasets(Dataset[] datasets) {
        this.datasets = datasets;
    }
}
