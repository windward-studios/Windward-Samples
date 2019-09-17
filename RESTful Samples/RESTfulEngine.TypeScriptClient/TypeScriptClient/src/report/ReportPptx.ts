import report = require("./Report");
import Report = report.Report;

export class ReportPptx extends Report{
    getOutputFormat(): string {
        return "pptx";
    }
}