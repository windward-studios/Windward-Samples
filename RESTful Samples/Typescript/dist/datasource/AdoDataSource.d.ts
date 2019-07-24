import { DataSource, DataSourceJson } from './DataSource';
/**
 * DataSource for data sources that support ADO (usually SQL)
 */
export declare class AdoDataSource extends DataSource {
    className: string;
    connectionString: string;
    /**
     * @param name Name of this data source; matches name used in template
     * @param className Class name this data source uses
     * e.g. "System.Data.SqlClient" for Microsoft Sql Server
     * @param connectionString Connection string used to connect to this data source.
     * e.g. "DataSource=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo"
     * would connect you to Windward's sample Sql Server database
     */
    constructor(name: string, className: string, connectionString: string);
    toJSON(): DataSourceJson;
}
export default AdoDataSource;
