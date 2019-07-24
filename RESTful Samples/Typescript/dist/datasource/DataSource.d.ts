import { TemplateVariable, VariableJson } from "./TemplateVariable";
/** interface used when sending json body to server */
export interface DataSourceJson {
    Name: string;
    Type: DataSourceType;
    ClassName?: string;
    ConnectionString?: string;
    Uri?: string;
    Data?: string;
    SchemaUri?: string;
    SchemaData?: string;
    Username?: string;
    Password?: string;
    Domain?: string;
    ODataVersion?: number;
    ODataProtocol?: ODataProtocolString;
    Variables: VariableJson[];
}
export declare type DataSourceType = "sql" | "xml" | "json" | "odata";
export declare type ODataProtocolString = "identity" | "basic" | "credentials" | "windowsauth";
/**
 * A data source to pass to the engine when running the report.
 */
export declare abstract class DataSource {
    name: string;
    variables: TemplateVariable[];
    /**
     * The default constructor for DataSource
     *
     * @param name Name of this DataSource, corresponding to the name used
     * when designing the template in AutoTag.
     */
    constructor(name: string);
    /**
     * Returns an object which conforms to the interface specified
     * by the RESTful Engine.  Used when sending the request to the server
     */
    abstract toJSON(): DataSourceJson;
    protected variablesToJSON(): VariableJson[];
}
export default DataSource;
