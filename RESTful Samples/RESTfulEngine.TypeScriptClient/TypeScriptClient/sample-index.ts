/* Home page for sample app using the RESTful Engine TypeScript client */

import express = require('express');
import fs = require('fs');
import Client = require("./src/Client");
import version = require("./src/Version");
import html = require("./src/report/ReportHtml");
import ReportHtml = html.ReportHtml;
import ado = require("./src/datasource/AdoDataSource");
import xml = require("./src/datasource/XmlDataSource");

var myClient = new Client.Client("localhost", 8080);
var engineVersion = "unkown version";

export function sampleIndex(req: express.Request, res: express.Response) {
    var report = createReportWithXmlDatasource();

    var finishedLoadingReportCallback = (response: string) => {
        console.log('report response' + response);
        res.render('index', { title: "RESTful Engine TypeScript Client Sample Application", version: engineVersion, reportOutput: response });
    };

    initializePage(req, res, () => {
         myClient.runReport(report, finishedLoadingReportCallback);
    });
};

function initializePage(req: express.Request, res: express.Response, next) {
    var finishedLoading = (version: version.Version) => {
        console.log('Version request complete, Engine Version: ' + version.EngineVersion);
        engineVersion = version.EngineVersion;

        next();
    };

    myClient.getVersion(finishedLoading);
}

function createReportWithAdoDatasource(): ReportHtml {
    var report = new ReportHtml(readFile("files/test-ado-datasource.docx"));

    var data = new ado.AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo");
    report.Datasources["MSSQL"] = data;

    return report;
}

function createReportWithXmlDatasource(): ReportHtml {
    var report = new ReportHtml(readFile("files/test-xml-datasource.docx"));

    var data = new xml.XmlDataSource(readFile("files/xml-test.xml"));
    report.Datasources["WRTRUCK1"] = data;

    return report;
}

function createReportWithNoDatasource(): ReportHtml {
    return new ReportHtml(readFile("files/test-no-datasource.docx"));
}

function readFile(filePath: string): Buffer {
    return fs.readFileSync(filePath);
}