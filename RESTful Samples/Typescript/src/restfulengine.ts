import fetch from 'isomorphic-fetch';

import {Version} from './Version';
import {Report, OutputFormat, Status, Hyphenation, CopyMetadataOption} from './Report';
import {DataSource} from './datasource/DataSource';
import {XmlDataSource} from './datasource/XmlDataSource';
import {AdoDataSource} from './datasource/AdoDataSource';
import {JsonDataSource} from './datasource/JsonDataSource';
import {ODataDataSource} from './datasource/ODataDataSource';
import {TemplateVariable} from './datasource/TemplateVariable';
import {DataSet} from './DataSet';

/**
 * Returns a promise that will resolve to the service and engine
 * version of the RESTful engine service running at the specified
 * location.
 *
 * @param baseUri URI of the RESTful Engine
 */
export function getVersion(baseUri:string):Promise<Version> {
    return fetch(normalizeBaseUri(baseUri) + "v1/version", {
        method: 'get',
        headers: {
            'Content-Type': 'application/json;charset=UTF-8',
            'Accept': 'application/json'
        }
    })
        .then((r:IResponse) => r.json<Version>());
}

/**
 * Instantiates and returns a `Report` object.
 *
 * @param baseUri URI of the RESTful Engine
 * @param outputFormat Desired `OutputFormat` of report
 * @param template Buffer holding the contents of the template file
 */
export function createReport(baseUri:string, outputFormat:OutputFormat, template:Buffer):Report;
/**
 * Instantiates and returns a `Report` object.
 *
 * @param baseUri URI of the RESTful Engine
 * @param outputFormat Desired `OutputFormat` of report
 * @param templateUri URI of the template accessible by the RESTful Engine server
 */
export function createReport(baseUri:string, outputFormat:OutputFormat, templateUri:string):Report;
export function createReport(baseUri:string, outputFormat:OutputFormat, template:Buffer|string):Report {
    return new Report(normalizeBaseUri(baseUri), outputFormat, template);
}

function normalizeBaseUri(baseUri:string):string {
    if (baseUri[baseUri.length - 1] !== "/") {
        return baseUri + "/";
    }
    return baseUri;
}

export {
    Version,
    Report,
    OutputFormat,
    Status,
    Hyphenation,
    CopyMetadataOption,
    DataSource,
    XmlDataSource,
    AdoDataSource,
    JsonDataSource,
    ODataDataSource,
    TemplateVariable,
    DataSet
};
