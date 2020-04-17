import inspect
import io
import os
import time

import restfulengine
from restfulengine import XmlDataSource
from restfulengine import AdoDataSource
from restfulengine import TemplateVariable
from restfulengine import Version
from restfulengine import OutputFormat
from restfulengine import Status


class TestClient:
    base_uri = "http://localhost:49731"

    script_loc = os.path.dirname(
        os.path.abspath(
            inspect.getfile(
                inspect.currentframe()
            )
        )
    ) + str(os.sep)
    test_data_rel_dir = "data" + os.sep  # relative to script
    test_data_dir = script_loc+test_data_rel_dir

    sample1_file = test_data_dir+"Sample1.docx"
    mfg_file = test_data_dir+"Manufacturing.docx"
    mfg_xml = test_data_dir+"Manufacturing.xml"
    mssql_file = test_data_dir+"MsSqlTemplate.docx"
    var_file = test_data_dir+"Variables.docx"

    def test_get_version(self):
        v = restfulengine.get_version(self.base_uri)
        assert isinstance(v, Version)
        assert len(v.engine_version) >= 7
        assert len(v.service_version) >= 7

    def test_post_template_returns_report_pdf(self):
        with open(self.sample1_file, 'rb') as template:
            output = io.BytesIO(b"")  # file-like object
            # initialize report and process
            report = restfulengine.create_report(
                self.base_uri,
                OutputFormat.pdf,
                template,
                output
            )
            report.process()
            # need 8 output bytes for file sig (ensure at begin);
            output.seek(0)
            output_data = output.read(8)
            assert len(output_data) == 8
            # PDF file should be "%PDF-1.5"
            assert output_data == b"%PDF-1.5"

    def test_post_template_with_xml_data(self):
        # set up data_sources
        with open(self.mfg_xml, 'rb') as xml_file:
            xml_data = xml_file.read()
        data_sources = [
            XmlDataSource("MANF_DATA_2009", xml_data)
        ]
        # initialize and process
        with open(self.mfg_file, 'rb') as template:
            output = io.BytesIO(b"")
            report = restfulengine.create_report(
                self.base_uri,
                OutputFormat.pdf,
                template,
                output
            )
            report.process(data_sources)
            # test is that we had no exceptions/errors

    def test_post_template_with_ado_data(self):
        # set up data_sources
        data_sources = [
            AdoDataSource(
                "MSSQL",
                "System.Data.SqlClient",
                "Data Source=mssql.windward.net;"
                "Initial Catalog=Northwind;"
                "User=demo;Password=demo")
        ]
        # initialize and process
        with open(self.mssql_file, "rb") as template:
            output = io.BytesIO(b"")
            report = restfulengine.create_report(
                self.base_uri,
                OutputFormat.pdf,
                template,
                output
            )
            report.process(data_sources)
            # test is that we had no exceptions/errors

    def test_variables(self):
        # set up data_sources with an xml data_source that has a variable
        with open(self.mfg_xml, 'rb') as xml_file:
            xml_data = xml_file.read()
        ds = XmlDataSource("", xml_data)
        ds.variables = [TemplateVariable("Var1", "hi there")]
        data_sources = [ds]
        # initialize and process
        with open(self.var_file, 'rb') as template:
            output = io.BytesIO(b"")
            report = restfulengine.create_report(
                self.base_uri,
                OutputFormat.pdf,
                template,
                output
            )
            report.process(data_sources)
            # test is that we had no exceptions/errors

    def test_post_template_async(self):
        # set up data_sources
        with open(self.mfg_xml, 'rb') as xml_file:
            xml_data = xml_file.read()
        data_sources = [XmlDataSource("MANF_DATA_2009", xml_data)]
        # initialize and process
        with open(self.mfg_file, 'rb') as template:
            report = restfulengine.create_report(
                self.base_uri,
                OutputFormat.pdf,
                template
            )
            report.process(data_sources)
        # now wait for report
        while report.get_status() == Status.working:
            time.sleep(0.1)

        assert report.get_status() == Status.ready
        report.get_report()
        report.delete()