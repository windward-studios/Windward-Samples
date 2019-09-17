import DataSource = require("./DataSource");
import Variable = require("./TemplateVariable");

export class AdoDataSource implements DataSource.DataSource {
    Variables: Variable.TemplateVariable[];
    connectionString: string;
    className: string;

    constructor(className: string, connectionString: string) {
        this.className = className;
        this.connectionString = connectionString;
    }

    GetJsonRequest() {
        var json = {
            Variables: this.Variables,
            Type: "sql",
            ClassName: this.className,
            connectionString: this.connectionString
        }
        return json;
    }

}
