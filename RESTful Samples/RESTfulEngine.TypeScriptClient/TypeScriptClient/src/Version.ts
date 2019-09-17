export class Version {
    ServiceVersion : string;
    EngineVersion: string;

    majorEngineVersion() {
        var major = this.EngineVersion.substring(0, 2);
        return parseInt(major);
    }

    majorServiceVersion() {
        var major = this.ServiceVersion.substring(0, 1);
        return parseInt(major);
    }
}