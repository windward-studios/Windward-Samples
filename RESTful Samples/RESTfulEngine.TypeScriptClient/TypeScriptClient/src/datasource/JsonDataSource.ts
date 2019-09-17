import DataSource = require("./DataSource");
import Variable = require("./TemplateVariable");

export class JsonDataSource implements DataSource.DataSource {
    Variables: Variable.TemplateVariable[];
    jsonFileData: Buffer = undefined;
    uri: string = undefined;

    constructor(data: string | Buffer) {
        if (typeof data === "string") {
            this.uri = data;
        }
        else
            this.jsonFileData = <Buffer>data;
    }

    GetJsonRequest() {
        var json = {
            Variables: JSON.stringify(this.Variables),
            Type: "json",
            URI: this.uri,
            Data: this.jsonFileData
        }
        return json;
    }


}
