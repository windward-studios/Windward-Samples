import Fs = require("fs");

export var testFilesDirectory = "../files/";

export function readFile(filePath: string): Buffer {
    return Fs.readFileSync(filePath);
}