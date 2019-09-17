import assert = require('assert');
import ado = require("../src/datasource/AdoDataSource");
import variable = require("../src/datasource/TemplateVariable");

export function TestSerializeTemplateVariables() {
    var data = new ado.AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo");
    data.Variables = [new variable.TemplateVariable("testvar", "hi")];

    var jsonRequest = data.GetJsonRequest();
    var strRequest = JSON.stringify(jsonRequest);

    assert.ok(strRequest.search("testvar") > 0, strRequest);
}

