export interface VariableJson {
    Name: string;
    Value: string;
    Type: VariableType;
}
export declare type VariableType = "text" | "int" | "float" | "datetime";
/**
 * Variables to be added to a data source for templates that use variables.
 * Note: Variables need to be added to the data source which is used by
 * the tag that utilizes this variable.  Sometimes this means adding variables
 * to multiple data sources.
 */
export declare class TemplateVariable {
    name: string;
    value: string;
    type: VariableType;
    /**
     * @param name Name of the variable as used in template
     * @param value Value to pass for this variable.
     * @param type Type of this variable -- one of "text"|"int"|"float"|"datetime" -- defaults to "text"
     */
    constructor(name: string, value: string, type?: VariableType);
    toJSON(): VariableJson;
}
export default TemplateVariable;
