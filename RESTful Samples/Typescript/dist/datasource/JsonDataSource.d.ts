import { DataSource, DataSourceJson } from './DataSource';
/**
 * DataSource for JSON data sources
 */
export declare class JsonDataSource extends DataSource {
    data: Buffer | string;
    username: string;
    password: string;
    domain: string;
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
    constructor(name: string, data: Buffer | string, username?: string, password?: string, domain?: string);
    toJSON(): DataSourceJson;
}
export default JsonDataSource;
