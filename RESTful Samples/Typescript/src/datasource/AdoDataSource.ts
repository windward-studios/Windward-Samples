import {DataSource, DataSourceJson} from './DataSource';

/**
 * DataSource for data sources that support ADO (usually SQL)
 */
export class AdoDataSource extends DataSource {
    /**
     * @param name Name of this data source; matches name used in template
     * @param className Class name this data source uses
     * e.g. "System.Data.SqlClient" for Microsoft Sql Server
     * @param connectionString Connection string used to connect to this data source.
     * e.g. "DataSource=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo"
     * would connect you to Windward's sample Sql Server database
     */
    public constructor(name:string, public className:string, public connectionString:string) {
        super(name);
    }

    public toJSON(): DataSourceJson {
        return {
            Name: this.name,
            Type: "sql",
            ClassName: this.className,
            ConnectionString: this.connectionString,
            Variables: this.variablesToJSON()
        };
    }
}

export default AdoDataSource;
