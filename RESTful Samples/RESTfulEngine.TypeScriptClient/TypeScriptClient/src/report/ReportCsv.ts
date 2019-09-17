import report = require("./Report");
import Report = report.Report;

export class ReportCsv extends Report{
    getOutputFormat(): string {
        return "csv";
    }
}