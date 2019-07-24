"use strict";
/**
 * DataSets to be used by templates; for more information, see
 * https://wiki.windward.net/Wiki/03.AutoTag/05.AutoTag_User_Guide/02.Data_Sources/Datasets
 * DataSets must be instantiated with either XML data or a URI
 * from which to retrieve XML data.  If both are specified, the data
 * will be used.  The XML data can be found in *.rdlx files
 */
var DataSet = (function () {
    /**
     * @param data Buffer encapsulating a DataSet file (*.rdlx) or a string
     * representation of a URI pointing to a DataSet file
     */
    function DataSet(data) {
        this.data = data;
    }
    DataSet.prototype.toJSON = function () {
        if (this.data instanceof Buffer) {
            return { Data: this.data.toString("base64") };
        }
        else {
            return { Uri: this.data };
        }
    };
    return DataSet;
}());
exports.DataSet = DataSet;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = DataSet;
