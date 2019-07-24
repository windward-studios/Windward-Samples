package client;

import com.sun.org.apache.xerces.internal.impl.dv.util.Base64;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import java.io.IOException;
import java.io.InputStream;
import java.net.URL;

/**
 * Created by Bassem on 4/12/2015.
 */
public class XmlDataSource extends DataSource {
    private InputStream data;
    private URL url;

    private InputStream schemaData;
    private URL schemaUrl;


    public XmlDataSource(InputStream data)
    {
        this.data = data;
    }

    public XmlDataSource(URL url)
    {
        this.url = url;
    }

    public void SetSchema(InputStream data)
    {
        schemaData = data;
    }

    public void SetSchema(URL url)
    {
        this.schemaUrl = url;
    }
    @Override
    public Element getXml(String name , Document doc) throws IOException {
        byte [] bytes = Utils.readAllBytes(data);
        Element dataSourceElement = doc.createElement("Datasource");

        Element nameElement = doc.createElement("Name");
        nameElement.appendChild(doc.createTextNode(name));
        dataSourceElement.appendChild(nameElement);

        Element typeElement = doc.createElement("Type");
        typeElement.appendChild(doc.createTextNode("xml"));
        dataSourceElement.appendChild(typeElement);

        if (data != null) {
            Element dataElement = doc.createElement("Data");
            dataElement.appendChild(doc.createTextNode(Base64.encode(bytes)));
            dataSourceElement.appendChild(dataElement);
        } else if (url != null) {
            Element urlElement = doc.createElement("Uri");
            urlElement.appendChild(doc.createTextNode(url.toString()));
            dataSourceElement.appendChild(urlElement);
        }

        if (schemaData != null) {
            Element dataElement = doc.createElement("SchemaData");
            dataElement.appendChild(doc.createTextNode(Base64.encode(Utils.readAllBytes(schemaData))));
            dataSourceElement.appendChild(dataElement);
        } else if (schemaUrl != null) {
            Element urlElement = doc.createElement("SchemaUri");
            urlElement.appendChild(doc.createTextNode(schemaUrl.toString()));
            dataSourceElement.appendChild(urlElement);
        }

        if(variables != null && variables.size() > 0){
            dataSourceElement.appendChild(getVariablesXml(doc));
        }

        return dataSourceElement;
    }
}
