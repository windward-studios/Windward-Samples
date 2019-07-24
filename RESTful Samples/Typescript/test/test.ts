require('isomorphic-fetch');
import * as chai from 'chai';
import * as fs from 'fs';
import * as path from 'path';
import {
    getVersion, createReport, OutputFormat, XmlDataSource, AdoDataSource,
    JsonDataSource, ODataDataSource, TemplateVariable, DataSet, Status, Version
} from 'restfulclient';

var assert = chai.assert;

suite("restfulclient", function() {
    this.timeout(60000);
    const baseUri = "http://localhost:49731";

    const testDataRelDir = "data" + path.sep;
    const testDataDir = __dirname + path.sep + testDataRelDir;

    const sample1File = testDataDir + "Sample1.docx";
    const mfgFile = testDataDir + "Manufacturing.docx";
    const mfgXml = testDataDir + "Manufacturing.xml";
    const mssqlFile = testDataDir + "MsSqlTemplate.docx";
    const varFile = testDataDir + "Variables.docx";
    const jsonFile = testDataDir + "JsonSample.docx";
    const odataFile = testDataDir + "ODataSample.docx";
    const datasetTemplate = testDataDir + "DataSet.docx";
    const datasetFile = testDataDir + "DataSet.rdlx";

    test("getVersion()", () => {
        getVersion(baseUri).then((v: Version) => {
            assert.isTrue(v.engineVersion.length >= 7);
            assert.isTrue(v.serviceVersion.length >= 7);
        });
    });

    test("template returns pdf", () => {
        var template = fs.readFileSync(sample1File),
            report = createReport(baseUri, OutputFormat.PDF, template);
        return report.process().then((output: Buffer) => {
            var outputData = output.toString('utf8', 0, 8);
            assert.equal(outputData, "%PDF-1.5");
        });
    });

    test("xml data source", () => {
        var xmlData = fs.readFileSync(mfgXml),
            template = fs.readFileSync(mfgFile),
            report = createReport(baseUri, OutputFormat.PDF, template);
        report.dataSources.push(
            new XmlDataSource("MANF_DATA_2009", xmlData)
        );
        return report.process();
        // test: no exceptions
    });

    test("ado data source", () => {
        var template = fs.readFileSync(mssqlFile),
            report = createReport(baseUri, OutputFormat.PDF, template);
        report.dataSources.push(
            new AdoDataSource("MSSQL", "System.Data.SqlClient",
                "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo"
            )
        );
        return report.process();
        // test: no exceptions
    });

    test("json data source", () => {
        var template = fs.readFileSync(jsonFile),
            report = createReport(baseUri, OutputFormat.PDF, template);
        report.dataSources.push(
            new JsonDataSource("", "http://json.windward.net/Northwind.json", "demo", "demo")
        );
        return report.process();
        // test: no exceptions
    });

    test("odata data source", () => {
        var template = fs.readFileSync(odataFile),
            report = createReport(baseUri, OutputFormat.PDF, template);
        report.dataSources.push(
            new ODataDataSource("", "http://odata.windward.net/Northwind/Northwind.svc", 2)
        );
        return report.process();
        // test: no exceptions
    });

    test("variables", () => {
        var template = fs.readFileSync(varFile),
            xmlData = fs.readFileSync(mfgXml),
            report = createReport(baseUri, OutputFormat.PDF, template),
            datasource = new XmlDataSource("", xmlData);
        datasource.variables.push(new TemplateVariable("Var1", "hi there"));
        report.dataSources.push(datasource);
        return report.process();
        // test: no exceptions
    });

    test("datasets", () => {
        var template = fs.readFileSync(datasetTemplate),
            datasetData = fs.readFileSync(datasetFile),
            report = createReport(baseUri, OutputFormat.PDF, template);
        report.dataSources.push(
            new AdoDataSource("", "System.Data.SqlClient",
                "Data Source=mssql.windward.net;Initial Catalog=AdventureWorks;User=demo;Password=demo")
        );
        report.dataSets.push(new DataSet(datasetData));
        return report.process();
        // test: no exceptions
    });

    test("async", () => {
        var template = fs.readFileSync(mfgFile),
            xmlData = fs.readFileSync(mfgXml),
            report = createReport(baseUri, OutputFormat.PDF, template);
        report.dataSources.push(
            new XmlDataSource("MANF_DATA_2009", xmlData)
        );
        report.processAsync();
        return report.getReportWhenReady()
        .then((output: Buffer) => report.deleteFromServer());
    });

});
