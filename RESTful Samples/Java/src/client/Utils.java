package client;

import org.w3c.dom.Document;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import java.io.*;
import java.util.ArrayList;
import java.util.List;

import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

/**
 * Created by Bassem on 4/12/2015.
 */
public class Utils {
    //get Byte array of document
    public static byte[] documentToByteArray(Document document) throws Exception {
        TransformerFactory tFactory =  TransformerFactory.newInstance();
        Transformer transformer = tFactory.newTransformer();
        DOMSource source = new DOMSource(document);
        ByteArrayOutputStream outStream = new ByteArrayOutputStream();
        StreamResult result = new StreamResult(outStream);
        transformer.transform(source, result);
        System.out.println(outStream.toString());
        return outStream.toByteArray();
    }

    public static Document stringToDocument(String xml) throws Exception{
        InputStream xmlStream =
                new ByteArrayInputStream(xml.getBytes("UTF-8"));
        DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
        dbf.setNamespaceAware(true);
        Document sourceDoc = dbf.newDocumentBuilder().parse(xmlStream);
        return sourceDoc;
    }

    public static byte[]readAllBytes(InputStream is) throws IOException{
        ByteArrayOutputStream buffer = new ByteArrayOutputStream();

        int nRead;
        byte[] data = new byte[16384];

        while ((nRead = is.read(data, 0, data.length)) != -1) {
            buffer.write(data, 0, nRead);
        }

        buffer.flush();

        return buffer.toByteArray();
    }

    public static String getXML(Document doc) throws Exception{
        TransformerFactory transfac = TransformerFactory.newInstance();
        Transformer trans = transfac.newTransformer();
        trans.setOutputProperty(OutputKeys.METHOD, "xml");
        trans.setOutputProperty(OutputKeys.INDENT, "yes");
        trans.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", Integer.toString(2));

        StringWriter sw = new StringWriter();
        StreamResult result = new StreamResult(sw);
        DOMSource source = new DOMSource(doc.getDocumentElement());

        trans.transform(source, result);
        String xmlString = sw.toString();
        return  xmlString;
    }

    public static List<String> getTextValuesByTagName(Element element, String tagName) {
        NodeList nodeList = element.getElementsByTagName(tagName);
        ArrayList<String> list = new ArrayList<String>();
        for (int i = 0; i < nodeList.getLength(); i++) {
            list.add(getTextValue(nodeList.item(i)));
        }
        return list;
    }
    public static String getTextValue(Node node) {
        StringBuffer textValue = new StringBuffer();
        int length = node.getChildNodes().getLength();
        for (int i = 0; i < length; i ++) {
            Node c = node.getChildNodes().item(i);
            if (c.getNodeType() == Node.TEXT_NODE) {
                textValue.append(c.getNodeValue());
            }
        }
        return textValue.toString().trim();
    }

}
