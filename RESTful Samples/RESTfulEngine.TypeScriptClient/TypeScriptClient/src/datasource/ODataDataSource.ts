import DataSource = require("./DataSource");
import Variable = require("./TemplateVariable");

export enum ODataProtocolType {
    NotProvided,
    Identity,
    Basic,
    Credentials,
    WindowsAuth
}

export class ODataDataSource implements DataSource.DataSource {

    Variables: Variable.TemplateVariable[];
    private uri: string;
    private domain: string;
    private username: string;
    private password: string;
    private version: number;
    private protocol: ODataProtocolType;

    constructor(uri: string, version: number, domain: string = undefined, username: string = undefined, password: string = undefined,
        protocol: ODataProtocolType = ODataProtocolType.NotProvided)
    {
        this.uri = uri;
        this.domain = domain;
        this.username = username;
        this.password = password;
        this.version = version;
        this.protocol = protocol;
    }

    GetJsonRequest() {
        var json = {
            Variables: this.Variables,
            Type: "odata",
            Uri: this.uri,
            Version: this.version,
            Domain: this.domain,
            Username: this.username,
            Protocol: ODataProtocolType[this.protocol].toString().toLowerCase()
        }
        return json;
    }

}
