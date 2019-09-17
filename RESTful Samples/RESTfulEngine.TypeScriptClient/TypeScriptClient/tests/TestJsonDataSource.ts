import assert = require('assert');
import Json = require("../src/datasource/JsonDataSource");
import variable = require("../src/datasource/TemplateVariable");
import Utils = require("../Utils");
var testFilesDirectory = "../files/";

export function TestSerializeWithUri() {
    var data = new Json.JsonDataSource("http://json.windward.net/Northwind.json");
    var jsonRequest = data.GetJsonRequest();
    var strRequest = JSON.stringify(jsonRequest);

    assert.ok(strRequest.search("URI") > 0, strRequest);
    assert.ok(strRequest.search("Data") < 0, "URI constructor contains 'Data' prop in json request");
}

export function TestSerializeWithLocalDataFile() {
    var data = new Json.JsonDataSource(Utils.readFile(testFilesDirectory + "Northwind.json"));
    var jsonRequest = data.GetJsonRequest();
    var strRequest = JSON.stringify(jsonRequest);

    assert.ok(strRequest.search("URI") < 0, "Local data file constructor contains 'URI' prop");
    assert.ok(strRequest.search("Data") > 0);
}

