import { DataSource, DataSourceJson } from './DataSource';
/**
 * DataSource for XML and other file-based data sources.
 */
export declare class XmlDataSource extends DataSource {
    data: Buffer | string;
    schema: Buffer | string;
    /**
     * Instantiates an XmlDataSource from either a Buffer encapsulating
     * an XML file, or a URI string pointing to an XML file.
     *
     * @param name Name of the DataSource as used in the template
     * @param data: Buffer or string representing an XMl file or a
     * URI pointing to one.
     * @param schema: Buffer or string representing an XMl file or a
     * URI pointing to one.  This file is used to determine the XML schema.
     */
    constructor(name: string, data: Buffer | string, schema?: Buffer | string);
    toJSON(): DataSourceJson;
}
export default XmlDataSource;
