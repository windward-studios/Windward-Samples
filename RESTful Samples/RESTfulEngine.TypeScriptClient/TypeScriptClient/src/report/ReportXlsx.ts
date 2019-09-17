import report = require("./Report");
import Report = report.Report;

export class ReportXlsx extends Report{
    getOutputFormat(): string {
        return "xlsx";
    }
}