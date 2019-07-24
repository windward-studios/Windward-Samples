import {DataSource, DataSourceJson} from "./datasource/DataSource";
import {DataSet, DataSetJson} from "./DataSet";

type OutputFormatString = "pdf" | "docx" | "xlsx" | "pptx" | "html" | "csv" | "rtf";
type FormatString = "docx" | "xlsx" | "pptx";
type CopyMetadataString = "never" | "nodatasource" | "always";
type HyphenateString = "off" | "template" | "on";

interface RequestJson {
    Data?: string;
    Uri?: string;
    OutputFormat?: OutputFormatString;
    Async?: boolean;
    Format?: FormatString;
    DataSources?: DataSourceJson[];
    DataSets?: DataSetJson[];
    CopyMetadata?: CopyMetadataString;
    Description?: string;
    Title?: string;
    Subject?: string;
    Keywords?: string;
    Hyphenate?: HyphenateString;
    Locale?: string;
    TrackImports?: boolean;
    Timeout?: number;
    RemoveUnusedFormats?: boolean;
}

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
export class Report {
    public baseUri:string;
    public template:Buffer|string;
    public asyncGuid:string;
    public outputFormat:OutputFormat;
    public dataSources:DataSource[];
    public dataSets:DataSet[];
    public description:string;
    public title:string;
    public subject:string;
    public keywords:string;
    public locale:string;
    public timeout:number;
    public hyphenate = Hyphenation.Template;
    public trackImports = false;
    public removeUnusedFormats = true;
    public copyMetaData = CopyMetadataOption.IfNoDataSource;


    /**
     * TODO
     * @param baseUri The base URI of your RESTful Engine server
     * @param outputFormat Your desired OutputFormat
     * @param template TODO
     */
    constructor(baseUri:string, outputFormat:OutputFormat, template:Buffer|string) {
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
    public process(dataSources?:DataSource[], dataSets?:DataSet[]):Promise<Buffer> {
        return new Promise((resolve:(value:Buffer) => void, reject:(error:Error) => void) => {
            this.fetchForProcess(false, dataSources, dataSets)
                .then((resp:IResponse) => {
                    if (resp.status !== 200) {
                        resp.text().then((t:string) => {
                            reject(new Error("process failed: " + t));
                        });
                    }
                    else {
                        return resp.json()
                    }
                })
                .then((json:{Data: string}) => {
                    if (json) {
                        resolve(new Buffer(json.Data, 'base64'));
                    }
                });
        });
    }

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
    public processAsync(dataSources?:DataSource[], dataSets?:DataSet[]) {
        this.fetchForProcess(true, dataSources, dataSets)
            .then((resp:IResponse) => resp.json())
            .then((json:{Guid: string}) => this.asyncGuid = json.Guid);
    }

    private fetchForProcess(async:boolean, dataSources?:DataSource[], dataSets?:DataSet[]):Promise<IResponse> {
        if (dataSources) this.dataSources.push.apply(this.dataSources, dataSources);
        if (dataSets) this.dataSets.push.apply(this.dataSets, dataSets);
        return fetch(this.baseUri + "v1/reports", {
            method: "POST",
            body: this.getJson(async),
            headers: {
                'Content-Type': 'application/json;charset=UTF-8',
                'Accept': 'application/json'
            }
        });
    }

    /**
     * For asynchronous reports, this method queries the service for a
     * status on this report.  For possible return values, see
     * `Status`.  This assumes you have already called processAsync().
     */
    public getStatus():Promise<Status> {
        if (!this.asyncGuid) {
            return new Promise(
                (resolve:(value:Status) => void) => {
                    resolve(Status.NotSent);
                }
            );
        }
        return fetch(`${this.baseUri}v1/reports/${this.asyncGuid}/status`)
            .then((resp:IResponse) => responseToStatus(resp));
    }

    /**
     * For asynchronous reports, retrieves the finished report
     * from the server.  If the report is not ready, this resolves
     * to a `Status` instead.
     */
    public getReport():Promise<Buffer|Status> {
        return new Promise((resolve:(value:Status|Buffer) => void, reject:(error?:Error) => void) => {
            if (!this.asyncGuid) {
                resolve(Status.NotSent);
            }
            fetch(`${this.baseUri}v1/reports/${this.asyncGuid}`, {
                headers: {
                    'Content-Type': 'application/json;charset=UTF-8',
                    'Accept': 'application/json'
                }
            })
                .then((resp:IResponse) => {
                    var status = responseToStatus(resp);
                    if (status === Status.Ready) {
                        resp.json().then((json:{Data: string}) => {
                            resolve(new Buffer(json.Data, "base64"));
                        });
                    }
                    if (status === Status.Working) {
                        resolve(status);
                    }
                    else { // error or not found
                        resp.text().then((text:string) => {
                            reject(new Error(text));
                        });
                    }
                }, reject);
        });
    }

    /**
     * For asynchronous reports, this method sends a DELETE message to
     * the service, which will subsequently delete this report from
     * the server.
     */
    public deleteFromServer() {
        fetch(`${this.baseUri}v1/reports/${this.asyncGuid}`, {
            method: "DELETE"
        }).then((resp:IResponse) => {
            if (resp.status !== 200) throw new Error("Error deleting!");
        });
    }

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
    public getReportWhenReady(numTries?:number, poll?:number):Promise<Buffer> {
        if (!numTries) numTries = 100; // don't allow 0
        if (poll == null) poll = 300; // compare against null because allow 0
        return new Promise((resolve:(value:Buffer) => void, reject:(error:Error) => void) => {
            this.tryGetReport(numTries, poll, resolve, reject);
        });
    }

    private tryGetReport(numTries:number, poll:number,
                         resolve:(value:Buffer) => void,
                         reject:(error:Error) => void) {
        if (numTries < 1) {
            reject(new Error("Maximum tries exceeded when trying to get report"));
        }
        this.getReport().then((value:Buffer | Status) => {
            if (value instanceof Buffer) resolve(value);
            else setTimeout(() => this.tryGetReport(numTries - 1, poll, resolve, reject), poll);
        }, reject);
    }

    /** Returns JSON for this report request */
    private getJson(async:boolean):string {
        var request:RequestJson = {
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
            request.DataSources = this.dataSources.map((dataSource:DataSource) => dataSource.toJSON());
        }
        if (this.dataSets.length > 0) {
            request.DataSets = this.dataSets.map((dataSet:DataSet) => dataSet.toJSON());
        }
        if (this.template instanceof Buffer) {
            request.Data = this.template.toString("base64");
        } else if (typeof this.template === "string") {
            request.Uri = <string>this.template;
        }

        return JSON.stringify(request);
    }
}

/**
 * Enum of the different possible output formats
 */
export enum OutputFormat {
    CSV,
    DOCX,
    HTML,
    PDF,
    PPTX,
    RTF,
    XLSX
}
function outputFormatToString(outputFormat:OutputFormat):OutputFormatString {
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
export enum Status {
    Ready,
    Working,
    Error,
    NotFound,
    NotSent
}
function responseToStatus(resp:IResponse) {
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
export enum Hyphenation {
    On,
    Off,
    Template
}
function hyphenateToString(hyphenate:Hyphenation):HyphenateString {
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
export enum CopyMetadataOption {
    IfNoDataSource,
    Never,
    Always
}
function copyMetadataToString(copyMetadata:CopyMetadataOption):CopyMetadataString {
    switch (copyMetadata) {
        case CopyMetadataOption.IfNoDataSource:
            return "nodatasource";
        case CopyMetadataOption.Never:
            return "never";
        case CopyMetadataOption.Always:
            return "always";
    }
}
