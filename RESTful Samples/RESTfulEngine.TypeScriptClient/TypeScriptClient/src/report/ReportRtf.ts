import report = require("./Report");
import Report = report.Report;

export class ReportRtf extends Report{
    getOutputFormat(): string {
        return "rtf";
    }
}