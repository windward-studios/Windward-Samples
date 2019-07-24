export interface VariableJson {
    Name: string;
    Value: string;
    Type: VariableType;
}

export type VariableType = "text" | "int" | "float" | "datetime";

/**
 * Variables to be added to a data source for templates that use variables.
 * Note: Variables need to be added to the data source which is used by
 * the tag that utilizes this variable.  Sometimes this means adding variables
 * to multiple data sources.
 */
export class TemplateVariable {
    /**
     * @param name Name of the variable as used in template
     * @param value Value to pass for this variable.
     * @param type Type of this variable -- one of "text"|"int"|"float"|"datetime" -- defaults to "text"
     */
    public constructor(public name:string, public value:string, public type?:VariableType) {
        if (!this.type) this.type = "text";
    }

    public toJSON(): VariableJson {
        return {
            Name: this.name,
            Value: this.value,
            Type: this.type
        }
    }
}

export default TemplateVariable;
