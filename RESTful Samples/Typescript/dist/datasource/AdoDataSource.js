"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var DataSource_1 = require('./DataSource');
/**
 * DataSource for data sources that support ADO (usually SQL)
 */
var AdoDataSource = (function (_super) {
    __extends(AdoDataSource, _super);
    /**
     * @param name Name of this data source; matches name used in template
     * @param className Class name this data source uses
     * e.g. "System.Data.SqlClient" for Microsoft Sql Server
     * @param connectionString Connection string used to connect to this data source.
     * e.g. "DataSource=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo"
     * would connect you to Windward's sample Sql Server database
     */
    function AdoDataSource(name, className, connectionString) {
        _super.call(this, name);
        this.className = className;
        this.connectionString = connectionString;
    }
    AdoDataSource.prototype.toJSON = function () {
        return {
            Name: this.name,
            Type: "sql",
            ClassName: this.className,
            ConnectionString: this.connectionString,
            Variables: this.variablesToJSON()
        };
    };
    return AdoDataSource;
}(DataSource_1.DataSource));
exports.AdoDataSource = AdoDataSource;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = AdoDataSource;
