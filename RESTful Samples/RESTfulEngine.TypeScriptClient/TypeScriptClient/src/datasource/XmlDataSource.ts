import DataSource = require("./DataSource");
import Variable = require("./TemplateVariable");

export class XmlDataSource implements DataSource.DataSource {
    Variables: Variable.TemplateVariable[];
    xmlData: Buffer = undefined;
    uri: string = undefined;

    schemaUri: string = undefined;
    schemaData: Buffer = undefined;

    constructor(data: string | Buffer, schema: string | Buffer = undefined) {
        if (typeof data === "string")
            this.uri = data;
        else
            this.xmlData = <Buffer>data;

        if (typeof schema === "string")
            this.schemaUri = schema;
        else
            this.schemaData = <Buffer>schema;
    }

    GetJsonRequest() {
        var json = {
            Variables: JSON.stringify(this.Variables),
            Type: "xml",

            Data: (this.xmlData == undefined ? undefined : this.xmlData.toString("base64")),
            Uri: this.uri,

            SchemaUri: this.schemaUri,
            SchemaData: (this.schemaData == undefined ? undefined : this.schemaData.toString("base64")),
        }

        return json;
    }
}
