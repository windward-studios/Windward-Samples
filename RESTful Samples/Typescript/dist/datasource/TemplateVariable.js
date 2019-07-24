"use strict";
/**
 * Variables to be added to a data source for templates that use variables.
 * Note: Variables need to be added to the data source which is used by
 * the tag that utilizes this variable.  Sometimes this means adding variables
 * to multiple data sources.
 */
var TemplateVariable = (function () {
    /**
     * @param name Name of the variable as used in template
     * @param value Value to pass for this variable.
     * @param type Type of this variable -- one of "text"|"int"|"float"|"datetime" -- defaults to "text"
     */
    function TemplateVariable(name, value, type) {
        this.name = name;
        this.value = value;
        this.type = type;
        if (!this.type)
            this.type = "text";
    }
    TemplateVariable.prototype.toJSON = function () {
        return {
            Name: this.name,
            Value: this.value,
            Type: this.type
        };
    };
    return TemplateVariable;
}());
exports.TemplateVariable = TemplateVariable;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = TemplateVariable;
