import report = require("./Report");
import Report = report.Report;

export class ReportHtml extends Report{
    getOutputFormat(): string {
        return "html";
    }
}