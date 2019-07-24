"use strict";
/**
 * A data source to pass to the engine when running the report.
 */
var DataSource = (function () {
    /**
     * The default constructor for DataSource
     *
     * @param name Name of this DataSource, corresponding to the name used
     * when designing the template in AutoTag.
     */
    function DataSource(name) {
        this.name = name;
        if (!name)
            name = "";
        this.name = name;
        this.variables = [];
    }
    DataSource.prototype.variablesToJSON = function () {
        return this.variables.map(function (variable) { return variable.toJSON(); });
    };
    return DataSource;
}());
exports.DataSource = DataSource;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = DataSource;
