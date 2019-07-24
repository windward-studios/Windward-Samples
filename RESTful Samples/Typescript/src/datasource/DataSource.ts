import {TemplateVariable, VariableJson} from "./TemplateVariable";

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

export type DataSourceType = "sql" | "xml" | "json" | "odata";
export type ODataProtocolString = "identity" | "basic" | "credentials" | "windowsauth";

/**
 * A data source to pass to the engine when running the report.
 */
export abstract class DataSource {
    public variables:TemplateVariable[];

    /**
     * The default constructor for DataSource
     *
     * @param name Name of this DataSource, corresponding to the name used
     * when designing the template in AutoTag.
     */
    public constructor(public name:string) {
        if (!name) name = "";
        this.name = name;
        this.variables = [];
    }

    /**
     * Returns an object which conforms to the interface specified
     * by the RESTful Engine.  Used when sending the request to the server
     */
    public abstract toJSON(): DataSourceJson;

    protected variablesToJSON(): VariableJson[] {
        return this.variables.map((variable: TemplateVariable) => variable.toJSON());
    }
}

export default DataSource;
