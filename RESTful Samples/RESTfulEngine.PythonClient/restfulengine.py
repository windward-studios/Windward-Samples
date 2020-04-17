# Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
#
# This software is the confidential and proprietary information of
# Windward Studios ("Confidential Information").  You shall not
# disclose such Confidential Information and shall use it only in
# accordance with the terms of the license agreement you entered into
# with Windward Studios, Inc.

"""
The Python Client for the RESTful Engine reporting service, this module
defines the following classes:

- :py:class:`Report`: represents a report and all the options
  associated with it. An object of this type is also used to retrieve
  output documents after report is processed.
- :py:class:`DataSource`: represents data sources that are to be used
  with the template.  This class in particular is meant to be an
  abstract base class of other DataSource classes such as
  :py:class:`AdoDataSource` and :py:class:`XmlDataSource`.
- :py:class:`AdoDataSource`: represents SQL-based datasources (using
  ADO.NET connectors).
- :py:class:`XmlDataSource`: represents file-based datasources,
  particularly, XML datasources.
- :py:class:`TemplateVariable`: represents variables associated with
  data sources in templates.
- :py:class:`Version`: used by :py:meth:`get_version()` to store
  version information.

Enumerations:

- :py:class:`OutputFormat`: A list of possible output formats
- :py:class:`Status`: Possible status codes returned from
  :py:meth:`Report.get_status()`
- :py:class:`Hyphenation`: Values indicating hyphenation settings in
  report
- :py:class:`CopyMetadataOption`: Values indicating whether or not to
  copy document metadata

Exception classes:

- :py:class:`ReportException`: Exception thrown by :py:meth:`Report.process()`

Functions:

- :py:meth:`get_version()`:
- :py:meth:`create_report()`:


How To Use This Module
======================

1. Import the module::

        import restfulengine

   or::

        from restfulengine import *

2. Create a :py:class:`Report` either by using its constructor or calling
   :py:meth:`create_report()`. You must specify a service URL, an output format
   and a template file (or URI). If you don't specify an output file, you are
   enabling asynchronous operation and later, must call
   :py:meth:`Report.get_report()` to retrieve the output::

        report = restfulengine.create_report(
            base_uri,
            OutputFormat.pdf,
            template
        )

3. Set up :py:class:`DataSource` objects and add them to a list.
   :py:class:`XmlDataSource` objects or
   :py:class:`AdoDataSource` objects::

        data_sources = [
            AdoDataSource(
                "MSSQL"
                "System.Data.SqlClient",
                "Data Source=mssql.windward.net;"
                "Initial Catalog=Northwind;"
                "User=demo;Password=demo")
        [

4. Call :py:meth:`Report.process()` with your data sources to send the request::

        report.process(data_sources)

5. (for async reports) call :py:meth:`Report.get_status()` to poll the server
   so you know when the report is done.  Then call
   :py:meth:`Report.get_report()` to retrieve the output.  Finally, call delete
   when you are done with the report to delete it from the server::

        while report.get_status() == Status.working:
            time.sleep(100)

        if report.get_status() == Status.ready:
            output = report.get_report()
            report.delete()

Members
=======
"""

import requests
import base64


_VERSION_PATH = "v1/version"
_REPORTS_PATH = "v1/reports"
_REPORT_GUID_PATH = _REPORTS_PATH + "/{}"  # parameter is guid
_STATUS_PATH = _REPORT_GUID_PATH + "/status"  # parameter is guid
_HEADERS = {'Content-Type': 'application/json;charset=UTR-8',
            'Accept': 'application/json'}


def get_version(base_uri):
    """ Returns the Service and Engine version of the RESTful engine
        service running at the specified location, or None if no
        reasonable response was obtained.

    :param base_uri: Location of service in URL form.
                     Example: "http://localhost:49731/"
    :type base_uri: str
    :return: A :py:class:`Version` object with the service and engine versions
    :rtype: Version
    """
    # Ensure we have a correctly formatted URL
    # Goal: "http://localhost:49731/"
    # Acceptable input: "http://localhost:49731"
    if not base_uri.endswith('/'):
        base_uri += '/'
    uri = base_uri+_VERSION_PATH
    r = requests.get(uri, headers=_HEADERS)
    if r.status_code == requests.codes.ok:
        return Version(r.json())
    else:
        return None


def create_report(base_uri, output_format, template, report=None):
    """ Creates a :py:class:`Report` object given a RESTful engine
        service URI and a template to process.  If no report parameter
        is specified, then the report will be created asynchronously.

    :param base_uri: URI of RESTful engine service
    :type base_uri: str
    :param output_format: Desired :py:class:`OutputFormat` for report.
    :type output_format: OutputFormat
    :param template: A file object with a template to be processed, or
        a string with a URI reference to the template file.
    :type template: file or string
    :param report: A file object to write resulting report to
    :type report: file
    """
    return Report(base_uri, output_format, template, report)


class Report:

    """
    This class contains several methods and parameters used to create
    and send a request to a Windward RESTful engine service, and
    receive and process the response.  The RESTful engine service
    supports asynchronous requests.  This will be initiated
    automatically if a report output file is not specified upon
    instantiation of the Report object.

    There are several options such as :py:attr:`description`,
    :py:attr:`title`, :py:attr:`timeout`, and :py:attr:`hyphenate`.
    When set, these options will be sent to the RESTful engine service
    with the template.  Each of these options is stored as an instance
    variable.  They are all initialized to None in the __init__
    constructor.

    After instantiation a Report object either through this
    constructor, or through the modules :py:meth:`create_report()`
    method, :py:meth:`process()` must be called in order to begin the
    processing. This is where you would also pass in a list of
    :py:class:`DataSource` objects

    For asynchronous requests, the output can be retrieved by using
    the :py:meth:`get_report()` method, or discarded using the
    :py:meth:`delete()` method.  While waiting for the service to
    finish processing the report, the status can be queried with the
    :py:meth:`get_status()` method.

    :param base_uri: URI of RESTful engine service
    :type base_uri: str
    :param output_format: Desired :py:class:`OutputFormat` for report.
    :type output_format: OutputFormat
    :param template: A file object with a template to be processed, or
        a URI pointing to a valid file
    :type template: file or string
    :param report: A file object to write resulting report to
    :type report: file
    """

    def __init__(self, base_uri, output_format, template, report=None):
        """
        :param base_uri: URI of RESTful engine service
        :type base_uri: str
        :param output_format: Desired :py:class:`OutputFormat` for report.
        :type output_format: OutputFormat
        :param template: A file object with a template to be processed.
            File object should be equivalent to something opened with
            mode="rb" so that it can be read as a binary file.
        :type template: file
        :param report: A file object to write resulting report to. If
            not specified, the report will be created as an
            asynchronous report. See :py:meth:`process()`. This should
            be a file object opened with 'wb' mode so it can be written
            to as a binary file.
        :type report: file
        """
        # need to normalize the URI
        # ("http://localhost:49731" -> "http://localhost:49731/")
        self._base_uri = base_uri
        if not self._base_uri.endswith('/'):
            self._base_uri += '/'
        self._template = template
        self._report = report
        self._guid = None

        # various options that can be set
        self.output_format = output_format
        """ :annotation: = Desired report output format
        :type: OutputFormat """
        self.description = None
        """ :type: str """
        self.title = None
        """ :type: str """
        self.subject = None
        """ :type: str """
        self.keywords = None
        """ :type: str """
        self.locale = None
        """ :type: str """
        self.timeout = 0
        """ :type: int """
        self.hyphenate = Hyphenation.template
        """ :type: Hyphenation """
        self.track_imports = False
        """ :type: bool """
        self.remove_unused_formats = True
        """ :type: bool """
        self.copy_meta_data = CopyMetadataOption.if_no_data_source
        """ :type: CopyMetadataOption """

    def process(self, data_sources=None):
        """ Puts together and sends the report request to the server.
        This also takes the data sources to send with the request. If
        an output file was specified, then this will write the results
        to that output file.  If no output file was specified, then
        this requests an asynchronous report which can later be
        retrieved with :py:meth:`get_report()`.

        :param data_sources: a dict of data sources for this report.
            The dict keys should correspond to the names of these data
            sources as used in the corresponding template.
        :type data_sources: DataSource[]
        """
        # we'll store the JSON for this request here
        try:
            req_json = {
                # read bytes from template, encode in base 64 and decode to str
                "Data": base64.b64encode(self._template.read()).decode("utf-8"),
                "OutputFormat": self.output_format
            }
        except AttributeError:
            req_json = {
                "Uri": self._template,
                "OutputFormat": self.output_format
            }

        # any data sources
        if data_sources is not None:
            req_json["Datasources"] = [ds.get_json() for ds in data_sources]

        # any optional options
        self._set_opt(req_json, "Description", self.description)
        self._set_opt(req_json, "Title", self.title)
        self._set_opt(req_json, "Subject", self.subject)
        self._set_opt(req_json, "Keywords", self.keywords)
        self._set_opt(req_json, "Locale", self.locale)
        self._set_opt(req_json, "Timeout", self.timeout)
        self._set_opt(req_json, "Hyphenate", self.hyphenate)
        self._set_opt(req_json, "TrackImports", self.track_imports)
        self._set_opt(req_json, "RemoveUnusedFormats", self.remove_unused_formats)
        self._set_opt(req_json, "CopyMetadata", self.copy_meta_data)

        # are we async?
        async = False
        if self._report is None:
            async = True
            self._set_opt(req_json, "Async", True)

        # make the request
        uri = self._base_uri + _REPORTS_PATH
        r = requests.post(uri, headers=_HEADERS, json=req_json)

        if async:
            self._handle_async_response(r)
        else:
            self._handle_response(r)

    def get_status(self):
        """
        For asynchronous reports, this method queries the service for a
        status on this report.  For possible return values, see
        :py:class:`Status`

        :return: Status of this report.
        :rtype: Status
        """
        uri = self._base_uri + _STATUS_PATH.format(self._guid)
        r = requests.get(uri)
        if r.status_code == requests.codes.ok:
            return Status.ready
        elif r.status_code == requests.codes.accepted:
            return Status.working
        elif r.status_code == requests.codes.internal_server_error:
            return Status.error
        else:
            return Status.not_found

    def get_report(self):
        """ For asynchronous reports, retrieves the finished report
        from the server.

        :return: A bytestring containing the report file
        :rtype: bytes
        """
        uri = self._base_uri + _REPORT_GUID_PATH.format(self._guid)
        r = requests.get(uri, headers=_HEADERS)
        if r.status_code == requests.codes.ok:
            return base64.b64decode(r.json()["Data"])

    def delete(self):
        """
        For asynchronous reports, this method sends a DELETE message to
        the service, which will subsequently delete this report from
        the server.
        """
        uri = self._base_uri + _REPORT_GUID_PATH.format(self._guid)
        r = requests.delete(uri)
        if r.status_code is not requests.codes.ok:
            raise ReportException(r.reason)

    # internal private methods
    def _set_opt(self, json, key, value):
        # Given a JSON object, key and value, sets key to value only
        # if value is not None
        if value is not None:
            json[key] = value

    def _handle_response(self, r):
        # this is for handling non-async response. We need to write it
        # out to self._report
        if r.status_code == requests.codes.ok:
            self._report.write(base64.b64decode(r.json()["Data"]))
        else:
            raise ReportException(r.json())

    def _handle_async_response(self, r):
        # this is for handling async response; just need GUID
        self._guid = r.json()["Guid"]


class DataSource:

    """ A data source to pass to the engine when running the report.
    This class is meant to be an abstract base class, and therefore
    some of its interfaces are non-functional (such as the
    :py:meth:`get_json()` method) without a subclass implementation.

    :param name: Name of this data source. This should match its name
        in the template.
    :type name: str
    """

    def __init__(self, name):
        self.name = name
        """ :type: str """
        self.variables = []
        """ :type: TemplateVariable[] """
        print("DataSource: Not yet!")  # TODO

    def get_json(self):
        """ Generates and returns a JSON representation of this data
        source. This is intended to be used when packaging up all
        required information for a report request by
        :py:meth:`Report.process()`
        :return: JSON dict of this data source intended for a request
        :rtype: dict
        """
        print("DataSource.get_json: Not yet!")  # TODO
        return ""

    def get_variables_json(self, json):
        """ Generates and adds to existing JSON a representation of the
        variables of this data source. This is intended to be used by
        subclass' :py:meth:`get_json()` implementations.
        """
        if len(self.variables) > 0:
            json["Variables"] = [var.get_json() for var in self.variables]


class AdoDataSource(DataSource):

    """ DataSource implementation for data sources that support ADO
    (usually SQL)

    :param name: Name of this data source. This should match its name
            in the template.
    :type name: str
    :param class_name: Class name this data source should use, e.g.
        "System.Data.SqlClient" for Microsoft Sql Server.
    :type class_name: str
    :param connection_string: Connection string.  e.g. "Data
        Source=mssql.windward.net;Initial Catalog=Northwind;
        User=demo;Password=demo" would connect you to Windward's
        sample database.
    """

    def __init__(self, name, class_name, connection_string):
        """
        :param name: Name of this data source. This should match its
            name in the template.
        :type name: str
        :param class_name: Class name this data source should use, e.g.
            "System.Data.SqlClient" for Microsoft Sql Server.
        :type class_name: str
        :param connection_string: Connection string.  e.g. "Data
            Source=mssql.windward.net;Initial Catalog=Northwind;
            User=demo;Password=demo" would connect you to Windward's
            sample database.
        """
        DataSource.__init__(self, name)
        self._class_name = class_name
        self._connection_string = connection_string

    def get_json(self):
        """ Generates and returns a JSON representation of this data
        source. This is intended to be used when packaging up all
        required information for a report request by
        :py:meth:`Report.process()`
        :return: JSON dict of this data source intended for a request
        :rtype: dict
        """
        ado_json =  {
            "Name": self.name,
            "Type": "sql",
            "ClassName": self._class_name,
            "ConnectionString": self._connection_string
        }
        self.get_variables_json(ado_json)
        return ado_json


class XmlDataSource(DataSource):

    """ DataSource implementation for XML and file-based data sources.
    This data source must be instantiated with either XML data or a URI
    from which to retrieve XML data.  If both are specified, the data
    will be used.

    :param name: Name of this data source. This should match its
        name in the template.
    :type name: str
    :param data: XML data as a bytestring.
    :type data: bytes
    :param uri: A URI from which to retrieve XML data (such as a
        web URL or file location) -- note, this URI must be
        accessible from the RESTful service which is being called.
    :type uri: str
    :param schema_data: XML data describing a schema to be used
        with the specified XML data.
    :type schema_data: bytes
    :param schema_uri: A URI for schema data.  See the description
        for the uri parameter
    :type schema_uri: str
    """

    def __init__(self,
                 name="",
                 data=None,
                 uri=None,
                 schema_data=None,
                 schema_uri=None):
        """
        :param name: Name of this data source. This should match its
            name in the template.
        :type name: str
        :param data: XML data as a bytestring.
        :type data: bytes
        :param uri: A URI from which to retrieve XML data (such as a
            web URL or file location) -- note, this URI must be
            accessible from the RESTful service which is being called.
        :type uri: str
        :param schema_data: XML data describing a schema to be used
            with the specified XML data.
        :type schema_data: bytes
        :param schema_uri: A URI for schema data.  See the description
            for the uri parameter
        :type schema_uri: str
        """
        DataSource.__init__(self, name)
        self._data = data
        self._uri = uri
        self._schema_data = schema_data
        self._schema_uri = schema_uri

        # enforce requirements
        if self._data is None and self._uri is None:
            raise ReportException("XmlDataSource objects must be instantiated"
                                  " with either a data string or a URI.")

    def get_json(self):
        """ Generates and returns a JSON representation of this data
        source. This is intended to be used when packaging up all
        required information for a report request by
        :py:meth:`Report.process()`
        :return: JSON dict of this data source intended for a request
        :rtype: dict
        """
        # The basics
        json = {
            "Name": self.name,
            "Type": "xml"
        }
        # not sure if data or URI.
        if self._data is not None:
            json["Data"] = base64.b64encode(self._data).decode("utf-8")
        elif self._uri is not None:
            json["Uri"] = self._uri
        # not sure if we got a schema data or URI; prefer data
        if self._schema_data is not None:
            json["SchemaData"] = base64.b64encode(self._schema_data).decode("utf-8")
        elif self._schema_uri is not None:
            json["SchemaUri"] = self._schema_uri

        self.get_variables_json(json)
        return json


class TemplateVariable:
    """ To be added to a data source if template requires variables

    :param name: Name of variable (must match variable in template)
    :type name: str
    :param value: Value to use in this variable when report is run
    :type value: str
    """
    def __init__(self, name, value):
        self.name = name
        """ :type: str """
        self.value = value
        """ :type: str """

    def get_json(self):
        """ Generates and returns a JSON representation of the
        variable intended for use by the subclass implementations of
        :py:meth:`DataSource.get_json()`.
        :return: a JSON representation of this variable
        :rtype: dict
        """
        return {"Name": self.name,
                "Value": self.value}


class Version:
    """ Class intended for use by :py:meth:`get_version()`.  It has two
    data members, one for the version of each the RESTful service, and
    the underlying Windward engine. The constructor takes the JSON
    response returned by a version call to the RESTful engine service,
    and parses it into a service version and engine version.

    :param json_resp: the response from an HTTP call to version
    :type json_resp: dict
    """
    def __init__(self, json_resp):
        self.service_version = json_resp["ServiceVersion"]
        """ :type: str """
        self.engine_version = json_resp["EngineVersion"]
        """ :type: str """


class ReportException(Exception):
    """ Exception class for the RESTful engine Python client class """
    def __init__(self, *args):
        Exception.__init__(self, *args)


class OutputFormat:
    """ Enum of the different possible output formats """
    csv = "csv"
    docx = "docx"
    html = "html"
    pdf = "pdf"
    pptx = "pptx"
    rtf = "rtf"
    xlsx = "xlsx"


class Status:
    """ Enum indicating status of a report """
    ready = 1
    working = 2
    error = 3
    not_found = 4


class Hyphenation:
    """ Enum indicating whether to turn hyphenation on or off """
    on = "on"
    off = "off"
    template = "template"


class CopyMetadataOption:
    """ Enum indicating whether to copy the document metadata to the
    report
    """
    if_no_data_source = "nodatasource"
    never = "never"
    always = "always"