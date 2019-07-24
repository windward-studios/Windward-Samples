/**
 * Intended for use by `getVersion()`. It has two data members, one for
 * the version of each the RESTful service and the underlying Windward
 * engine.
 */
export interface Version {
    serviceVersion: string;
    engineVersion: string;
}

export default Version;
