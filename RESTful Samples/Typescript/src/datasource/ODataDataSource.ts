import {DataSource, DataSourceJson, ODataProtocolString} from './DataSource';

/**
 * OData Data Source
 */
export class ODataDataSource extends DataSource {
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
    public constructor(name:string,
                       public uri:string,
                       public version:number,
                       public username?:string,
                       public password?:string,
                       public domain?:string,
                       public protocol?:Protocol) {
        super(name);

        if (!this.version) this.version = 1;
        if (!this.protocol) this.protocol = Protocol.Identity;
    }

    public toJSON(): DataSourceJson {
        return {
            Name: this.name,
            Type: "odata",
            Uri: this.uri,
            Username: this.username,
            Password: this.password,
            Domain: this.domain,
            ODataVersion: this.version,
            ODataProtocol: protocolToString(this.protocol),
            Variables: this.variablesToJSON()
        }
    }
}

export enum Protocol {
    Identity,
    Basic,
    Credentials,
    WindowsAuth
}
function protocolToString(protocol:Protocol):ODataProtocolString {
    return <ODataProtocolString>Protocol[protocol].toLowerCase();
}

export default ODataDataSource;
