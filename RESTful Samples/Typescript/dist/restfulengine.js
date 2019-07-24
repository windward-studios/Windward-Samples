"use strict";
var Report_1 = require('./Report');
exports.Report = Report_1.Report;
exports.OutputFormat = Report_1.OutputFormat;
exports.Status = Report_1.Status;
exports.Hyphenation = Report_1.Hyphenation;
exports.CopyMetadataOption = Report_1.CopyMetadataOption;
var DataSource_1 = require('./datasource/DataSource');
exports.DataSource = DataSource_1.DataSource;
var XmlDataSource_1 = require('./datasource/XmlDataSource');
exports.XmlDataSource = XmlDataSource_1.XmlDataSource;
var AdoDataSource_1 = require('./datasource/AdoDataSource');
exports.AdoDataSource = AdoDataSource_1.AdoDataSource;
var JsonDataSource_1 = require('./datasource/JsonDataSource');
exports.JsonDataSource = JsonDataSource_1.JsonDataSource;
var ODataDataSource_1 = require('./datasource/ODataDataSource');
exports.ODataDataSource = ODataDataSource_1.ODataDataSource;
var TemplateVariable_1 = require('./datasource/TemplateVariable');
exports.TemplateVariable = TemplateVariable_1.TemplateVariable;
var DataSet_1 = require('./DataSet');
exports.DataSet = DataSet_1.DataSet;
/**
 * Returns a promise that will resolve to the service and engine
 * version of the RESTful engine service running at the specified
 * location.
 *
 * @param baseUri URI of the RESTful Engine
 */
function getVersion(baseUri) {
    return fetch(normalizeBaseUri(baseUri) + "v1/version", {
        method: 'get',
        headers: {
            'Content-Type': 'application/json;charset=UTF-8',
            'Accept': 'application/json'
        }
    })
        .then(function (r) { return r.json(); });
}
exports.getVersion = getVersion;
function createReport(baseUri, outputFormat, template) {
    return new Report_1.Report(normalizeBaseUri(baseUri), outputFormat, template);
}
exports.createReport = createReport;
function normalizeBaseUri(baseUri) {
    if (baseUri[baseUri.length - 1] !== "/") {
        return baseUri + "/";
    }
    return baseUri;
}
