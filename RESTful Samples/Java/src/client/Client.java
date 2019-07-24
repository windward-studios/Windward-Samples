package client;

import org.w3c.dom.Document;

import java.io.*;
import java.net.HttpURLConnection;
import java.net.URL;

/**
 * Created by Bassem on 4/12/2015.
 */
public class Client {

    public static class OutputData{
        public Document outputXMl;
        public int statusCode;

        public OutputData(Document outputXMl,int statusCode){
            this.outputXMl = outputXMl;
            this.statusCode = statusCode;
        }
    }

    public static OutputData post(URL url, Document body) throws Exception{
        HttpURLConnection request = createPostRequest(url);
        setRequestBody(request, body);
        return processRequest(request);
    }

    public static OutputData get(URL url)throws Exception
    {
        HttpURLConnection request = createGetRequest(url);
        return processRequest(request);
    }

    public static OutputData delete(URL url) throws Exception
    {
        HttpURLConnection request = createDeleteRequest(url);
        return processRequest(request);
    }

    private static void setRequestBody(HttpURLConnection request, Document body) throws Exception{
        String xml= Utils.getXML(body);
        byte []bytes = xml.getBytes();//TODO any problem??
        int contentLength = bytes.length;
        request.setRequestProperty("Content-Length", "" + contentLength);
        request.setDoOutput(true);
        DataOutputStream wr = new DataOutputStream(request.getOutputStream());
        wr.writeBytes(new String(bytes));//TODO here
        wr.flush();
        wr.close();
    }

    private static OutputData processRequest(HttpURLConnection request)throws Exception   {
        int code = request.getResponseCode();
        Document doc = GetResponseBody(request);
        return new OutputData(doc, code);
    }


    private static Document GetResponseBody(HttpURLConnection response) throws Exception{
        if (response.getContentLength() <= 0)
            return null;

        StringBuffer sb;
        Document body = null;
        try {
            // Read the response's contents, either normal data or error.
            BufferedReader buffReader;
            InputStream errorStream = response.getErrorStream();
            if (errorStream != null)
                buffReader = new BufferedReader( new InputStreamReader( errorStream ) );
            else
                buffReader = new BufferedReader( new InputStreamReader( response.getInputStream() ) );
            sb = new StringBuffer();
            String inputLine;
            while ((inputLine = buffReader.readLine()) != null) {
                sb.append( inputLine );    // receive the response xml document
            }

            // We've got an error.
            if (errorStream != null)
                throw new ReportException(sb.toString());

            // We've got an XML document.
            if(sb.length() > 0)
                body = Utils.stringToDocument(sb.toString());
        } catch (Exception e) {
            if (e instanceof ReportException)
                throw e; // Re-throw.
            else
                throw new ReportException("Exception in GetResponse Body: "+e.getMessage()+"\nHttp status code: "+response.getResponseCode(), e);
        }

        return body;
    }


    private static HttpURLConnection createPostRequest(URL url)throws IOException{
        HttpURLConnection conn = createRequest(url, "POST");
        return conn;
    }

    private static HttpURLConnection createGetRequest(URL url)throws IOException{
        return createRequest(url, "GET");
    }

    private static HttpURLConnection createDeleteRequest(URL url) throws IOException{
        return createRequest(url, "DELETE");
    }

    private static HttpURLConnection createRequest(URL url, String method) throws IOException{
        HttpURLConnection con;
        con = (HttpURLConnection) url.openConnection();
        con.setRequestMethod(method);
        con.setRequestProperty("Content-Type", "application/xml");
//        request.Accept = "application/xml";
        return con;
    }

}
