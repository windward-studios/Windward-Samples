package client;

import org.w3c.dom.Document;
import org.w3c.dom.Element;

import java.io.IOException;
import java.util.List;

/**
 * Created by Bassem on 4/12/2015.
 */
/// <summary>
/// The base class for all data source providers.
/// </summary>
public abstract class DataSource
{
    protected List<TemplateVariable> variables ;

    public List<TemplateVariable> getVariables() {
        return variables;
    }

    public void setVariables(List<TemplateVariable> variables) {
        this.variables = variables;
    }

    protected Element getVariablesXml(Document doc)
    {
        Element variablesElement = doc.createElement("Variables");
        for(TemplateVariable var : variables){
            Element variableElement = doc.createElement("Variable");
            variablesElement.appendChild(variableElement);

            Element nameElement = doc.createElement("Name");
            nameElement.appendChild(doc.createTextNode(var.getName()));
            variableElement.appendChild(nameElement);

            Element valElement = doc.createElement("Value");
            valElement.appendChild(doc.createTextNode(var.getValue()));
            variableElement.appendChild(valElement);
        }
        return variablesElement;
    }

    public abstract Element getXml(String name,Document document) throws IOException;
}