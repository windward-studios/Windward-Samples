import { Version } from './Version';
import { Report, OutputFormat, Status, Hyphenation, CopyMetadataOption } from './Report';
import { DataSource } from './datasource/DataSource';
import { XmlDataSource } from './datasource/XmlDataSource';
import { AdoDataSource } from './datasource/AdoDataSource';
import { JsonDataSource } from './datasource/JsonDataSource';
import { ODataDataSource } from './datasource/ODataDataSource';
import { TemplateVariable } from './datasource/TemplateVariable';
import { DataSet } from './DataSet';
/**
 * Returns a promise that will resolve to the service and engine
 * version of the RESTful engine service running at the specified
 * location.
 *
 * @param baseUri URI of the RESTful Engine
 */
export declare function getVersion(baseUri: string): Promise<Version>;
/**
 * Instantiates and returns a `Report` object.
 *
 * @param baseUri URI of the RESTful Engine
 * @param outputFormat Desired `OutputFormat` of report
 * @param template Buffer holding the contents of the template file
 */
export declare function createReport(baseUri: string, outputFormat: OutputFormat, template: Buffer): Report;
/**
 * Instantiates and returns a `Report` object.
 *
 * @param baseUri URI of the RESTful Engine
 * @param outputFormat Desired `OutputFormat` of report
 * @param templateUri URI of the template accessible by the RESTful Engine server
 */
export declare function createReport(baseUri: string, outputFormat: OutputFormat, templateUri: string): Report;
export { Version, Report, OutputFormat, Status, Hyphenation, CopyMetadataOption, DataSource, XmlDataSource, AdoDataSource, JsonDataSource, ODataDataSource, TemplateVariable, DataSet };
