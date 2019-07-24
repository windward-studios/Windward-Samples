package client;

/**
 * Created by Bassem on 4/12/2015.
 */
public class TemplateVariable {

    public String name,value;

    public TemplateVariable(String name,String value){
        this.name = name;
        this.value = value;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getValue() {
        return value;
    }

    public void setValue(String value) {
        this.value = value;
    }
}
