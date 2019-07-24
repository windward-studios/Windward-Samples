import { DataSource, DataSourceJson } from './DataSource';
/**
 * OData Data Source
 */
export declare class ODataDataSource extends DataSource {
    uri: string;
    version: number;
    username: string;
    password: string;
    domain: string;
    protocol: Protocol;
    /**
     * Instantiates an OData Data Source.  Pay special attention
     * to the version and protocol options which have defaults if
     * they are not specified.
     *
     * @param name Name of the data source as used in the template
     * @param uri URI of the OData service to connect to
     * @param version Version of the OData service. Defaults to 1
     * @param username
     * @param password
     * @param domain
     * @param protocol: Authentication protocol to use. Defaults to "identity"
     */
    constructor(name: string, uri: string, version: number, username?: string, password?: string, domain?: string, protocol?: Protocol);
    toJSON(): DataSourceJson;
}
export declare enum Protocol {
    Identity = 0,
    Basic = 1,
    Credentials = 2,
    WindowsAuth = 3,
}
export default ODataDataSource;
