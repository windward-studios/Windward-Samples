# Windward Reports RESTFul Engine TypeScript client example
See sample-index.ts for example application usage of the client code. 

## Running the example

## Requirements
- Node.js installed and in your system path var
- Typescript 1.5 installed and in your system path var

### From the Command  Line
Note: you must be in the TypeScriptClient sub folder when running these command line instructions.

1. Start a RESTFul engine server running on port 8080

2. Install the app's node dependencies with the following command:
`npm install`

3. Compile the app with the following command:
`tsc`

4. Launch the Node process to serve the app using the following command:
`node app-typescript.js`

5. Open your favorite browser and going to the following URL to access the app:
`http://localhost:1337/`

### From Visual Studio 2015
1. Install the Node.js Tools and TypeScript 1.5.4 Visual Studio extensions

2. Start a RESTFul engine server running on port 8080

3. Open the RESTFulEngine.TypeScript_Client.sln solution file and press F5 to run it

## Example Client Code Usage

Ex. Report with no datasource
	var myClient = new Client.WrClient("localhost", 8080);
	var report = new Html.ReportHtml(readFile(testFilesDirectory + "test-no-datasource.docx"));

    myClient.runReport(report, callbackFunction);


Ex. Report with Ado Data Source
    var report = new Html.ReportHtml(templateFileBuffer);
	
	// (ado class, connection string)
    var data = new Source.AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo");

    // set the template name for your datasource
	report.Datasources["MSSQL"] = data;

    myClient.runReport(report, callbackFunction);


Ex. Template Variables
    var report = new Html.ReportHtml(templateFileBuffer);
    var data = new Source.AdoDataSource("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo");

    /* Pass in template variables here */
    data.Variables = [new TemplateVariable("order", "10538")];

	report.Datasources["MSSQL"] = data;
    myClient.runReport(report, callbackFunction);
