package client;

import org.w3c.dom.Document;
import org.w3c.dom.Element;

import java.io.IOException;

/**
 * Created by Bassem on 4/14/2015.
 */
public class AdoDataSource extends DataSource {
    private String className;
    private String connectionString;

    public AdoDataSource(String className, String connectionString)
    {
        this.className = className;
        this.connectionString = connectionString;
    }
    @Override
    public Element getXml(String name, Document doc) throws IOException {
        Element dataSourceElement = doc.createElement("Datasource");

        Element nameElement = doc.createElement("Name");
        nameElement.appendChild(doc.createTextNode(name));
        dataSourceElement.appendChild(nameElement);

        Element typeElement = doc.createElement("Type");
        typeElement.appendChild(doc.createTextNode("sql"));
        dataSourceElement.appendChild(typeElement);

        Element classNameElement = doc.createElement("ClassName");
        classNameElement.appendChild(doc.createTextNode(className));
        dataSourceElement.appendChild(classNameElement);

        Element conStgElement = doc.createElement("ConnectionString");
        conStgElement.appendChild(doc.createTextNode(connectionString));
        dataSourceElement.appendChild(conStgElement);

        if(variables != null && variables.size() > 0){
            dataSourceElement.appendChild(getVariablesXml(doc));
        }

        return dataSourceElement;
    }
}
