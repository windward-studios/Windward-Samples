"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var DataSource_1 = require('./DataSource');
/**
 * DataSource for XML and other file-based data sources.
 */
var XmlDataSource = (function (_super) {
    __extends(XmlDataSource, _super);
    /**
     * Instantiates an XmlDataSource from either a Buffer encapsulating
     * an XML file, or a URI string pointing to an XML file.
     *
     * @param name Name of the DataSource as used in the template
     * @param data: Buffer or string representing an XMl file or a
     * URI pointing to one.
     * @param schema: Buffer or string representing an XMl file or a
     * URI pointing to one.  This file is used to determine the XML schema.
     */
    function XmlDataSource(name, data, schema) {
        _super.call(this, name);
        this.data = data;
        this.schema = schema;
        this.data = data;
        this.schema = schema;
    }
    XmlDataSource.prototype.toJSON = function () {
        var json = {
            Name: this.name,
            Type: "xml",
            Variables: this.variablesToJSON()
        };
        if (this.data instanceof Buffer) {
            json.Data = this.data.toString("base64");
        }
        else {
            json.Uri = this.data;
        }
        if (!this.schema) {
            return json;
        }
        if (this.schema instanceof Buffer) {
            json.SchemaData = this.schema.toString("base64");
        }
        else {
            json.SchemaUri = this.schema;
        }
        return json;
    };
    return XmlDataSource;
}(DataSource_1.DataSource));
exports.XmlDataSource = XmlDataSource;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = XmlDataSource;
