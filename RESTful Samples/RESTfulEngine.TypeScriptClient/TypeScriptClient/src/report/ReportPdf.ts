import report = require("./Report");
import Report = report.Report;

export class ReportPdf extends Report{
    getOutputFormat(): string {
        return "pdf";
    }
}