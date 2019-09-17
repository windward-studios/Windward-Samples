import report = require("./Report");
import Report = report.Report;

export class ReportDocx extends Report{
    getOutputFormat(): string {
        return "docx";
    }
}