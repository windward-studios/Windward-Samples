import { DataSource } from "./datasource/DataSource";
import { DataSet } from "./DataSet";
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
export declare class Report {
    baseUri: string;
    template: Buffer | string;
    asyncGuid: string;
    outputFormat: OutputFormat;
    dataSources: DataSource[];
    dataSets: DataSet[];
    description: string;
    title: string;
    subject: string;
    keywords: string;
    locale: string;
    timeout: number;
    hyphenate: Hyphenation;
    trackImports: boolean;
    removeUnusedFormats: boolean;
    copyMetaData: CopyMetadataOption;
    /**
     * TODO
     * @param baseUri The base URI of your RESTful Engine server
     * @param outputFormat Your desired OutputFormat
     * @param template TODO
     */
    constructor(baseUri: string, outputFormat: OutputFormat, template: Buffer | string);
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
    process(dataSources?: DataSource[], dataSets?: DataSet[]): Promise<Buffer>;
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
    processAsync(dataSources?: DataSource[], dataSets?: DataSet[]): void;
    private fetchForProcess(async, dataSources?, dataSets?);
    /**
     * For asynchronous reports, this method queries the service for a
     * status on this report.  For possible return values, see
     * `Status`.  This assumes you have already called processAsync().
     */
    getStatus(): Promise<Status>;
    /**
     * For asynchronous reports, retrieves the finished report
     * from the server.  If the report is not ready, this resolves
     * to a `Status` instead.
     */
    getReport(): Promise<Buffer | Status>;
    /**
     * For asynchronous reports, this method sends a DELETE message to
     * the service, which will subsequently delete this report from
     * the server.
     */
    deleteFromServer(): void;
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
    getReportWhenReady(numTries?: number, poll?: number): Promise<Buffer>;
    private tryGetReport(numTries, poll, resolve, reject);
    /** Returns JSON for this report request */
    private getJson(async);
}
/**
 * Enum of the different possible output formats
 */
export declare enum OutputFormat {
    CSV = 0,
    DOCX = 1,
    HTML = 2,
    PDF = 3,
    PPTX = 4,
    RTF = 5,
    XLSX = 6,
}
/**
 * Enum indicating status of a report
 */
export declare enum Status {
    Ready = 0,
    Working = 1,
    Error = 2,
    NotFound = 3,
    NotSent = 4,
}
/**
 * Enum indicating whether to turn hyphenation on or off
 */
export declare enum Hyphenation {
    On = 0,
    Off = 1,
    Template = 2,
}
/**
 * Enum indicating whether to copy the document metadata to the report
 */
export declare enum CopyMetadataOption {
    IfNoDataSource = 0,
    Never = 1,
    Always = 2,
}
