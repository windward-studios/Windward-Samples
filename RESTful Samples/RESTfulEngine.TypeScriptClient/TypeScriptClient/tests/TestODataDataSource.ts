import assert = require('assert');
import client = require("../src/Client")
import Html = require("../src/report/ReportHtml");
import OData = require("../src/datasource/ODataDataSource");
import variable = require("../src/datasource/TemplateVariable");
import Utils = require("../Utils");
var myClient = new client.Client("localhost", 8080);


export function TestSerializeBasic() {
    var data = new OData.ODataDataSource("http://odata.windward.net/Northwind/Northwind.svc ", 2);
    var jsonRequest = data.GetJsonRequest();
    var strRequest = JSON.stringify(jsonRequest);

    assert.ok(strRequest.search("Uri") > 0, strRequest);
    assert.ok(strRequest.search("Version") > 0, strRequest);
    assert.ok(strRequest.search("Password") < 0, strRequest);
}


export function TestRunODataReport() {
    var report = new Html.ReportHtml(Utils.readFile(Utils.testFilesDirectory + "test-odata-datasource.docx"));
    var data = new OData.ODataDataSource("http://odata.windward.net/Northwind/Northwind.svc", 2);
    report.Datasources[""] = data;

    myClient.runReport(report, (response: string) => {
        assert.ok(response.search("Davolio") > 0, response);
    });
}
