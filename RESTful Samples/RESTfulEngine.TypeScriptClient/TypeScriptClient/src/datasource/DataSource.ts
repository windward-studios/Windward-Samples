import Variable = require("./TemplateVariable");

export interface DataSource {
    Variables: Variable.TemplateVariable[];
    GetJsonRequest();
}
