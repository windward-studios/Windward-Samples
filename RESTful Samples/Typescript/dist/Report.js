"use strict";
/**
 * This class contains several methods and parameters used to create
 * and send a request to a Windward RESTful engine service, and
 * receive and process the response.  The RESTful engine service
 * supports asynchronous requests.  This will be initiated
 * automatically if a report output file is not specified upon
 * instantiation of a `Report`.
 *
 * There are several options such as `description`,
 * `title`, `timeout`, and `hyphenate`.
 * When set, these options will be sent to the RESTful engine service
 * with the template.  Each of these options is stored as an instance
 * variable.
 *
 * After instantiation a `Report` either through the
 * constructor, or through the modules `createReport()`
 * method, `process()` must be called in order to begin the
 * processing. This is where you would also pass in a list of
 * `DataSource` and `DataSet` objects.  Alternatively, data sources
 * and data sets could be specified by setting the `dataSources` and
 * `dataSets` properties on `Report` appropriately.
 *
 * For asynchronous requests, the output can be retrieved by using
 * the `getReport()` method, or discarded using the
 * `deleteFromServer()` method.  While waiting for the service to
 * finish processing the report, the status can be queried with the
 * `getStatus()` method.
 */
var Report = (function () {
    /**
     * TODO
     * @param baseUri The base URI of your RESTful Engine server
     * @param outputFormat Your desired OutputFormat
     * @param template TODO
     */
    function Report(baseUri, outputFormat, template) {
        this.hyphenate = Hyphenation.Template;
        this.trackImports = false;
        this.removeUnusedFormats = true;
        this.copyMetaData = CopyMetadataOption.IfNoDataSource;
        this.baseUri = baseUri;
        this.outputFormat = outputFormat;
        this.template = template;
        this.dataSources = [];
        this.dataSets = [];
    }
    /**
     * Puts together and sends the report request to the server.
     * The output is returned as a Node.js Buffer.
     *
     * Takes arrays of `DataSource` and `DataSet` objects.
     * Alternatively, datasources and datasets may be set up by
     * adding them to the respectively named properties.  If DataSources
     * and DataSets are already set on the `Report` object and are also
     * passed in to process, the ones passed in to process will be
     * appended to the existing list.
     *
     * @param dataSources An array of `DataSource` objects for this report.
     * @param dataSets An array of `DataSet` objects for this report.
     *
     */
    Report.prototype.process = function (dataSources, dataSets) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.fetchForProcess(false, dataSources, dataSets)
                .then(function (resp) {
                if (resp.status !== 200) {
                    resp.text().then(function (t) {
                        reject(new Error("process failed: " + t));
                    });
                }
                else {
                    return resp.json();
                }
            })
                .then(function (json) {
                if (json) {
                    resolve(new Buffer(json.Data, 'base64'));
                }
            });
        });
    };
    /**
     * Asynchronously sends a report request to the server. The output
     * can be checked on with `getStatus()` and later retrieved with
     * `getReport()`.
     *
     * Takes arrays of `DataSource` and `DataSet` objects.
     * Alternatively, datasources and datasets may be set up by
     * adding them to the respectively named properties.  If DataSources
     * and DataSets are already set on the `Report` object and are also
     * passed in to process, the ones passed in to process will be
     * appended to the existing list.
     *
     * @param dataSources An array of `DataSource` objects for this report.
     * @param dataSets An array of `DataSet` objects for this report.
     */
    Report.prototype.processAsync = function (dataSources, dataSets) {
        var _this = this;
        this.fetchForProcess(true, dataSources, dataSets)
            .then(function (resp) { return resp.json(); })
            .then(function (json) { return _this.asyncGuid = json.Guid; });
    };
    Report.prototype.fetchForProcess = function (async, dataSources, dataSets) {
        if (dataSources)
            this.dataSources.push.apply(this.dataSources, dataSources);
        if (dataSets)
            this.dataSets.push.apply(this.dataSets, dataSets);
        return fetch(this.baseUri + "v1/reports", {
            method: "POST",
            body: this.getJson(async),
            headers: {
                'Content-Type': 'application/json;charset=UTF-8',
                'Accept': 'application/json'
            }
        });
    };
    /**
     * For asynchronous reports, this method queries the service for a
     * status on this report.  For possible return values, see
     * `Status`.  This assumes you have already called processAsync().
     */
    Report.prototype.getStatus = function () {
        if (!this.asyncGuid) {
            return new Promise(function (resolve) {
                resolve(Status.NotSent);
            });
        }
        return fetch(this.baseUri + "v1/reports/" + this.asyncGuid + "/status")
            .then(function (resp) { return responseToStatus(resp); });
    };
    /**
     * For asynchronous reports, retrieves the finished report
     * from the server.  If the report is not ready, this resolves
     * to a `Status` instead.
     */
    Report.prototype.getReport = function () {
        var _this = this;
        return new Promise(function (resolve, reject) {
            if (!_this.asyncGuid) {
                resolve(Status.NotSent);
            }
            fetch(_this.baseUri + "v1/reports/" + _this.asyncGuid, {
                headers: {
                    'Content-Type': 'application/json;charset=UTF-8',
                    'Accept': 'application/json'
                }
            })
                .then(function (resp) {
                var status = responseToStatus(resp);
                if (status === Status.Ready) {
                    resp.json().then(function (json) {
                        resolve(new Buffer(json.Data, "base64"));
                    });
                }
                if (status === Status.Working) {
                    resolve(status);
                }
                else {
                    resp.text().then(function (text) {
                        reject(new Error(text));
                    });
                }
            }, reject);
        });
    };
    /**
     * For asynchronous reports, this method sends a DELETE message to
     * the service, which will subsequently delete this report from
     * the server.
     */
    Report.prototype.deleteFromServer = function () {
        fetch(this.baseUri + "v1/reports/" + this.asyncGuid, {
            method: "DELETE"
        }).then(function (resp) {
            if (resp.status !== 200)
                throw new Error("Error deleting!");
        });
    };
    /**
     * For asynchronous reports, this method will continually poll the
     * server to retrieve the status of this report until it is ready.
     * Returns a Promise which will resolve to the Buffer of this
     * report's output.
     *
     * @param numTries Number of times to poll the server before giving
     *                 up.  Defaults to 100
     * @param poll Number of milliseconds to insert between server hits.
     *                 Defaults to 300
     */
    Report.prototype.getReportWhenReady = function (numTries, poll) {
        var _this = this;
        if (!numTries)
            numTries = 100; // don't allow 0
        if (poll == null)
            poll = 300; // compare against null because allow 0
        return new Promise(function (resolve, reject) {
            _this.tryGetReport(numTries, poll, resolve, reject);
        });
    };
    Report.prototype.tryGetReport = function (numTries, poll, resolve, reject) {
        var _this = this;
        if (numTries < 1) {
            reject(new Error("Maximum tries exceeded when trying to get report"));
        }
        this.getReport().then(function (value) {
            if (value instanceof Buffer)
                resolve(value);
            else
                setTimeout(function () { return _this.tryGetReport(numTries - 1, poll, resolve, reject); }, poll);
        }, reject);
    };
    /** Returns JSON for this report request */
    Report.prototype.getJson = function (async) {
        var request = {
            Async: async,
            OutputFormat: outputFormatToString(this.outputFormat),
            Description: this.description,
            Title: this.title,
            Subject: this.subject,
            Keywords: this.keywords,
            Locale: this.locale,
            Timeout: this.timeout,
            Hyphenate: hyphenateToString(this.hyphenate),
            TrackImports: this.trackImports,
            RemoveUnusedFormats: this.removeUnusedFormats,
            CopyMetadata: copyMetadataToString(this.copyMetaData)
        };
        if (this.dataSources.length > 0) {
            request.DataSources = this.dataSources.map(function (dataSource) { return dataSource.toJSON(); });
        }
        if (this.dataSets.length > 0) {
            request.DataSets = this.dataSets.map(function (dataSet) { return dataSet.toJSON(); });
        }
        if (this.template instanceof Buffer) {
            request.Data = this.template.toString("base64");
        }
        else if (typeof this.template === "string") {
            request.Uri = this.template;
        }
        return JSON.stringify(request);
    };
    return Report;
}());
exports.Report = Report;
/**
 * Enum of the different possible output formats
 */
(function (OutputFormat) {
    OutputFormat[OutputFormat["CSV"] = 0] = "CSV";
    OutputFormat[OutputFormat["DOCX"] = 1] = "DOCX";
    OutputFormat[OutputFormat["HTML"] = 2] = "HTML";
    OutputFormat[OutputFormat["PDF"] = 3] = "PDF";
    OutputFormat[OutputFormat["PPTX"] = 4] = "PPTX";
    OutputFormat[OutputFormat["RTF"] = 5] = "RTF";
    OutputFormat[OutputFormat["XLSX"] = 6] = "XLSX";
})(exports.OutputFormat || (exports.OutputFormat = {}));
var OutputFormat = exports.OutputFormat;
function outputFormatToString(outputFormat) {
    switch (outputFormat) {
        case OutputFormat.CSV:
            return "csv";
        case OutputFormat.DOCX:
            return "docx";
        case OutputFormat.HTML:
            return "html";
        case OutputFormat.PDF:
            return "pdf";
        case OutputFormat.PPTX:
            return "pptx";
        case OutputFormat.RTF:
            return "rtf";
        case OutputFormat.XLSX:
            return "xlsx";
    }
}
/**
 * Enum indicating status of a report
 */
(function (Status) {
    Status[Status["Ready"] = 0] = "Ready";
    Status[Status["Working"] = 1] = "Working";
    Status[Status["Error"] = 2] = "Error";
    Status[Status["NotFound"] = 3] = "NotFound";
    Status[Status["NotSent"] = 4] = "NotSent";
})(exports.Status || (exports.Status = {}));
var Status = exports.Status;
function responseToStatus(resp) {
    switch (resp.status) {
        case 200:
            return Status.Ready;
        case 202:
            return Status.Working;
        case 404:
            return Status.NotFound;
        default:
            return Status.Error;
    }
}
/**
 * Enum indicating whether to turn hyphenation on or off
 */
(function (Hyphenation) {
    Hyphenation[Hyphenation["On"] = 0] = "On";
    Hyphenation[Hyphenation["Off"] = 1] = "Off";
    Hyphenation[Hyphenation["Template"] = 2] = "Template";
})(exports.Hyphenation || (exports.Hyphenation = {}));
var Hyphenation = exports.Hyphenation;
function hyphenateToString(hyphenate) {
    switch (hyphenate) {
        case Hyphenation.On:
            return "on";
        case Hyphenation.Off:
            return "off";
        case Hyphenation.Template:
            return "template";
    }
}
/**
 * Enum indicating whether to copy the document metadata to the report
 */
(function (CopyMetadataOption) {
    CopyMetadataOption[CopyMetadataOption["IfNoDataSource"] = 0] = "IfNoDataSource";
    CopyMetadataOption[CopyMetadataOption["Never"] = 1] = "Never";
    CopyMetadataOption[CopyMetadataOption["Always"] = 2] = "Always";
})(exports.CopyMetadataOption || (exports.CopyMetadataOption = {}));
var CopyMetadataOption = exports.CopyMetadataOption;
function copyMetadataToString(copyMetadata) {
    switch (copyMetadata) {
        case CopyMetadataOption.IfNoDataSource:
            return "nodatasource";
        case CopyMetadataOption.Never:
            return "never";
        case CopyMetadataOption.Always:
            return "always";
    }
}
