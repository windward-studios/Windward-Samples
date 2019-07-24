"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var DataSource_1 = require('./DataSource');
/**
 * OData Data Source
 */
var ODataDataSource = (function (_super) {
    __extends(ODataDataSource, _super);
    /**
     * Instantiates an OData Data Source.  Pay special attention
     * to the version and protocol options which have defaults if
     * they are not specified.
     *
     * @param name Name of the data source as used in the template
     * @param uri URI of the OData service to connect to
     * @param version Version of the OData service. Defaults to 1
     * @param username
     * @param password
     * @param domain
     * @param protocol: Authentication protocol to use. Defaults to "identity"
     */
    function ODataDataSource(name, uri, version, username, password, domain, protocol) {
        _super.call(this, name);
        this.uri = uri;
        this.version = version;
        this.username = username;
        this.password = password;
        this.domain = domain;
        this.protocol = protocol;
        if (!this.version)
            this.version = 1;
        if (!this.protocol)
            this.protocol = Protocol.Identity;
    }
    ODataDataSource.prototype.toJSON = function () {
        return {
            Name: this.name,
            Type: "odata",
            Uri: this.uri,
            Username: this.username,
            Password: this.password,
            Domain: this.domain,
            ODataVersion: this.version,
            ODataProtocol: protocolToString(this.protocol),
            Variables: this.variablesToJSON()
        };
    };
    return ODataDataSource;
}(DataSource_1.DataSource));
exports.ODataDataSource = ODataDataSource;
(function (Protocol) {
    Protocol[Protocol["Identity"] = 0] = "Identity";
    Protocol[Protocol["Basic"] = 1] = "Basic";
    Protocol[Protocol["Credentials"] = 2] = "Credentials";
    Protocol[Protocol["WindowsAuth"] = 3] = "WindowsAuth";
})(exports.Protocol || (exports.Protocol = {}));
var Protocol = exports.Protocol;
function protocolToString(protocol) {
    return Protocol[protocol].toLowerCase();
}
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = ODataDataSource;
