import assert = require('assert');
import client = require("../src/Client")
import version = require("../src/Version")
import Fs = require("fs");
import Html = require("../src/report/ReportHtml");
import ReportHtml = Html.ReportHtml;
import Ado = require("../src/datasource/AdoDataSource");
import AdoDataSource = Ado.AdoDataSource;
import Xml = require("../src/datasource/XmlDataSource");
import XmlDataSource = Xml.XmlDataSource;
import variable = require("../src/datasource/TemplateVariable");
import TemplateVariable = variable.TemplateVariable;
import Json = require("../src/datasource/JsonDataSource");
import JsonDataSource = Json.JsonDataSource;
import Utils = require("../Utils");

var testClient = new client.Client("localhost", 8080);
var TEST_FILES_DIRECTORY = "../files/";

export function TestGetVersionRequest() {
    testClient.getVersion((version: version.Version) => {
        assert.ok(version.majorEngineVersion() > 13);
    });
}

export function TestRunReportNoDataSource() {
    var report = new ReportHtml(Utils.readFile(TEST_FILES_DIRECTORY + "test-no-datasource.docx"));

    testClient.runReport(report, (response: string) => {
        assert.ok(response.search("Test") > 0);
    });
}

export function TestReportAdoDataSource() {
    var report = new ReportHtml(Utils.readFile(TEST_FILES_DIRECTORY + "test-ado-datasource.docx"));
    var data = new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo");
    report.Datasources["MSSQL"] = data;

    testClient.runReport(report, (response: string) => {
        assert.ok(response.search("Davolio") > 0);
    });
}

export function TestRunReportXmlDataSource() {
    var report = new ReportHtml(Utils.readFile(TEST_FILES_DIRECTORY + "test-xml-datasource.docx"));
    var data = new XmlDataSource(Utils.readFile(TEST_FILES_DIRECTORY + "xml-test.xml"));
    report.Datasources["WRTRUCK1"] = data;

    testClient.runReport(report, (response: string) => {
        assert.ok(response.search("Windward Dashboard Summary Report") > 0, response);
    });
}

export function TestRunReportJsonDataSource() {
    var report = new ReportHtml(Utils.readFile(TEST_FILES_DIRECTORY + "test-json-datasource.docx"));
    var data = new JsonDataSource("http://json.windward.net/Northwind.json");
    report.Datasources[""] = data;

    testClient.runReport(report, (response: string) => {
        assert.ok(response.search("Davolio") > 0, response);
    });
}

export function TestRunReportWithTemplateVariables() {
    var report = new ReportHtml(Utils.readFile(TEST_FILES_DIRECTORY + "test-template-variables.docx"));
    var data = new AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo");
    data.Variables = [new TemplateVariable("order", "10538")];

    report.Datasources["MSSQL"] = data;

    testClient.runReport(report, (response: string) => {
        assert.ok(response.search("10538") > 0);
    });
}

export function TestRunReportAsync() {
    var report = new ReportHtml(Utils.readFile(TEST_FILES_DIRECTORY + "test-no-datasource.docx"));

    testClient.runReportAsync(report, (guid: string) => {
        assert.ok(guid.length === 36);
    });
}

export function TestRunReportAsyncAndGetReport() {
    var report = new ReportHtml(Utils.readFile(TEST_FILES_DIRECTORY + "test-no-datasource.docx"));

    testClient.runReportAsync(report, (guid: string) => {
        assert.ok(guid.length === 36);
        sleep(5);

        testClient.getReport(guid, (response) => {
            assert.ok(response.search("Test") > 0);
        });
    });
}

// don't use this in a production app
function sleep(seconds) {
    var e = new Date().getTime() + (seconds * 1000);
    while (new Date().getTime() <= e) { }
}