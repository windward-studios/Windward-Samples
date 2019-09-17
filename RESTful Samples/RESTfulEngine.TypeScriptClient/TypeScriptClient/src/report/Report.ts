import DataSourceModule = require("../datasource/DataSource");
import DataSource = DataSourceModule.DataSource;

export enum HyphenationType {
    On,
    Off,
    Template
}

export enum CopyMetadataOption {
    /// Copy the Windward metadata to the output report if no datasources were applied to the report. This is the default.
    NoDatasource,

    /// Never copy the Windward metadata to the output report.
    Never,

    /// Always copy the Windward metadata to the output report.
    Always
}

export class Report {
    templateData: Buffer;
    outputPath: string;
    Description: string;
    Title: string;
    Subject: string;
    Keywords: string;
    Locale: string;
    Timeout: number;
    Hypenation: HyphenationType = HyphenationType.Off;
    MetadataOption: CopyMetadataOption = CopyMetadataOption.NoDatasource;

    Datasources: { [key: string]: DataSource; } = {};

    constructor(templateData: Buffer, outputPath: string = "") {
        this.templateData = templateData;
        this.outputPath = outputPath;
    }

    getOutputFormat(): string {
        return "html";
    }

    CreateRequest() {
        var request = {
            Data: this.templateData.toString("base64"),
            OutputFormat: this.getOutputFormat(),
            Async: false,
            Description: this.Description,
            Title: this.Title,
            Subject: this.Subject,
            Keywords: this.Keywords,
            Locale: this.Locale,
            Hyphenate: HyphenationType[this.Hypenation].toString().toLowerCase(),
            Timeout: this.Timeout,
            CopyMetaData: CopyMetadataOption[this.MetadataOption].toString().toLowerCase(),
            DataSources: []
        }
        for (var key in this.Datasources) {
            var datasourceRequest = this.Datasources[key].GetJsonRequest();
            datasourceRequest.Name = key;
            request.DataSources.push(datasourceRequest);
        }
        return request;
    }
}
