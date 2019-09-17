import http = require('http');

import versionModule = require("./Version");
import Version = versionModule.Version;

import reportModule = require("./report/Report");
import Report = reportModule.Report;

export class Client {
    uri: string;
    port: number;
    constructor(uri: string, port: number = 8080) {
        this.uri = uri;
        this.port = port;
    }

    getVersion(complete: (version: Version) => any) {
        this.getRequest("/v1/version", (data) => {
            var version = new Version();
            version.EngineVersion = data.EngineVersion;
            version.ServiceVersion = data.ServiceVersion;
            complete(version);
        });
    }

    runReport(report: Report, complete: (response: string) => any) {
        var template = JSON.stringify(report.CreateRequest());

        this.postRequest("/v1/reports", template, (data, error) => {
            var output;
            if (error) {
                output = error;
            } else {
                var dataOutput = data.Data;
                output = new Buffer(dataOutput, "base64").toString("ascii");
            }

            complete(output);
        });
    }

    runReportAsync(report: Report, complete: (guid: string) => any) {
        var request = report.CreateRequest();
        request.Async = true;
        var template = JSON.stringify(request);

        this.postRequest("/v1/reports", template, (data) => {
            var guid = data.Guid;
            complete(guid);
        });
    }

    getReport(reportGuid: string, complete: (response: string) => any) {
        this.getRequest("/v1/reports/" + reportGuid, (data) => {
            var dataOutput = data.Data;
            var output = new Buffer(dataOutput, "base64").toString("ascii");

            complete(output);
        });
    }

    getRequest(path: string, dataCallback: (data) => any) {
        var options = {
            host: this.uri,
            port: this.port,
            path: path,
            method: "GET",
            headers: { 'Content-Type': "application/json" }
        };

        var callback = response => {
            var str = "";
            response.on("data", chunk => {
                str += chunk;
            });

            response.on("end", () => {
                dataCallback(JSON.parse(str));

            });
        }
        http.request(options, callback).end();
    }

    postRequest(path: string, postData: string, dataCallback: (data, error) => any) {
        var options = {
            host: this.uri,
            port: this.port,
            path: path,
            method: "POST",
            headers: {
                'Content-Type': "application/json",
                'Content-Length': postData.length
            },
            body: postData
        };

        var request = http.request(options, response => {
            var data = "";
            response.on("data", chunk => {
                data += chunk;
            });

            response.on("end", () => {
                if (response.statusCode !== 200) {
                    var errorMessage =
                        `RESTFul Engine Client Error: ${response.statusCode} ${response.statusMessage} <br />`;

                    errorMessage += data.replace(/\\r?\\n/g, "<br />");

                    console.log(errorMessage);
                    dataCallback(data, errorMessage);
                }
                else
                    dataCallback(JSON.parse(data.toString()), null);
            });
        });

        request.write(postData);
        request.end();
    }

} 