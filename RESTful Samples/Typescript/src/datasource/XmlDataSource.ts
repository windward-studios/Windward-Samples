import {DataSource, DataSourceJson} from './DataSource';

/**
 * DataSource for XML and other file-based data sources.
 */
export class XmlDataSource extends DataSource {

    /**
     * Instantiates an XmlDataSource from either a Buffer encapsulating
     * an XML file, or a URI string pointing to an XML file.
     *
     * @param name Name of the DataSource as used in the template
     * @param data: Buffer or string representing an XMl file or a
     * URI pointing to one.
     * @param schema: Buffer or string representing an XMl file or a
     * URI pointing to one.  This file is used to determine the XML schema.
     */
    public constructor(name: string, public data: Buffer | string, public schema?: Buffer | string) {
        super(name);
        this.data = data;
        this.schema = schema;
    }

    public toJSON(): DataSourceJson {
        var json: DataSourceJson = {
            Name: this.name,
            Type: "xml",
            Variables: this.variablesToJSON()
        };

        if (this.data instanceof Buffer) {
            json.Data = this.data.toString("base64");
        }
        else {
            json.Uri = <string>this.data;
        }

        if (!this.schema) {
            return json;
        }

        if (this.schema instanceof Buffer) {
            json.SchemaData = this.schema.toString("base64");
        }
        else {
            json.SchemaUri = <string>this.schema;
        }


        return json;
    }
}

export default XmlDataSource;
