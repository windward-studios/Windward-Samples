import {DataSource, DataSourceJson} from './DataSource';

/**
 * DataSource for JSON data sources
 */
export class JsonDataSource extends DataSource {
    /**
     * Instantiates a JsonDataSource with either a Buffer representing
     * a JSON file or a string URI that points to a JSON file.
     *
     * @param name Name of the data source as used in the template
     * @param data Buffer or string URI representing/pointing to a JSON file
     * @param username
     * @param password
     * @param domain
     */
    public constructor(name:string,
                       public data:Buffer | string,
                       public username?:string,
                       public password?:string,
                       public domain?:string) {
        super(name);
    }

    public toJSON(): DataSourceJson {
        var json: DataSourceJson = {
            Name: this.name,
            Type: "json",
            Username: this.username,
            Password: this.password,
            Domain: this.domain,
            Variables: this.variablesToJSON()
        };

        if (this.data instanceof Buffer) {
            json.Data = this.data.toString("base64");
        }
        else {
            json.Uri = <string>this.data;
        }
        return json;
    }
}

export default JsonDataSource;
