"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var DataSource_1 = require('./DataSource');
/**
 * DataSource for JSON data sources
 */
var JsonDataSource = (function (_super) {
    __extends(JsonDataSource, _super);
    /**
     * Instantiates a JsonDataSource with either a Buffer representing
     * a JSON file or a string URI that points to a JSON file.
     *
     * @param name Name of the data source as used in the template
     * @param data Buffer or string URI representing/pointing to a JSON file
     * @param username
     * @param password
     * @param domain
     */
    function JsonDataSource(name, data, username, password, domain) {
        _super.call(this, name);
        this.data = data;
        this.username = username;
        this.password = password;
        this.domain = domain;
    }
    JsonDataSource.prototype.toJSON = function () {
        var json = {
            Name: this.name,
            Type: "json",
            Username: this.username,
            Password: this.password,
            Domain: this.domain,
            Variables: this.variablesToJSON()
        };
        if (this.data instanceof Buffer) {
            json.Data = this.data.toString("base64");
        }
        else {
            json.Uri = this.data;
        }
        return json;
    };
    return JsonDataSource;
}(DataSource_1.DataSource));
exports.JsonDataSource = JsonDataSource;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = JsonDataSource;
