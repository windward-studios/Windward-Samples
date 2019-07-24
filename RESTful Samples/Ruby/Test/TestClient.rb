require 'test/unit/ui/console/testrunner'
require 'test/unit'
require 'uri'
require_relative '..\report'
require_relative '..\reportpdf'
require_relative '..\XmlDataSource'
require_relative '..\AdoDataSource'
require_relative '..\TemplateVariable'
require_relative '..\Report'

class TestClient < Test::Unit::TestCase

	@uri
	def setup()
		@uri = URI("http://localhost:8080")
	end
	def test_GetVersion()
		v = Report::GetVersion @uri
		assert(v != nil)
		assert(v.EngineVersion.is_a?(String))
		assert(v.ServiceVersion.is_a?(String))
	end
	
	def test_PostTemplateReturnsReportPdf
		output = StringIO.new()
		template = File.open('TestFiles\Sample1.docx', 'rb')
		report = ReportPdf.new(@uri, template, output)
		report.Process()
		assert(output.size > 8)
		bytes = Array.new
		bytes = output.string[0, 8]
		expected = ['25', '50', '44', '46', '2d', '31', '2e', '35']
		for i in 0..7
			assert((bytes[i]).unpack('H*')[0] == expected[i])
		end
	end
	
	def test_PostTemplateWithXmlData()
		xmlFile = File.open('TestFiles\Manufacturing.xml', 'rb')
		dataSources = {"MANF_DATA_2009" => XmlDataSource.new(xmlFile)}
		templateFilePath = 'TestFiles\Manufacturing.docx';
		outputFilePath = 'TestFiles\XmlDataOutput.pdf';
		templateFile = File.open(templateFilePath, 'rb')
		outputFile = File.new(outputFilePath, 'wb')
		report = ReportPdf.new(@uri, templateFile, outputFile)
		report.ProcessDataSources(dataSources)
	
	end
	
	def test_PostTemplateWithAdoData()
		dataSources = {"MSSQL" => AdoDataSource.new("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo")}
		templateFilePath = 'TestFiles\MsSqlTemplate.docx';
        outputFilePath = 'TestFiles\AdoDataOutput.pdf';
		templateFile = File.open(templateFilePath, 'rb')
		outputFile = File.new(outputFilePath, 'wb')
		report = ReportPdf.new(@uri, templateFile, outputFile)
		report.ProcessDataSources(dataSources)
	end
	
	def test_ClientVariables()
		ds = XmlDataSource.new(File.open('TestFiles\\Manufacturing.xml', 'rb'))
		templateVars = [TemplateVariable.new("var1", "Hi there")]
		ds.variables = templateVars
		dataSources = {"MANF_DATA_2009" => ds}
		templateFile = File.open('TestFiles\Variables.docx', 'r')
		outputFile = File.new('TestFiles\VariablesOutput.pdf', 'w')
		report = ReportPdf.new(@uri, templateFile, outputFile)
		report.ProcessDataSources(dataSources)
	end
	
	def test_PostTemplateAsync()
		xmlFile = File.open('TestFiles\\Manufacturing.xml')
		dataSources = {"MANF_DATA_2009" => XmlDataSource.new(xmlFile)}
		templateFilePath = 'TestFiles\Manufacturing.docx';
		outputFilePath = 'TestFiles\AsyncOutput.pdf';
		templateFile = File.open(templateFilePath, 'rb')
		report = ReportPdf.new(@uri, templateFile)
		report.ProcessDataSources(dataSources)
		
		while(report.GetStatus == :Working)
			sleep(100)
		end
		
		if(report.GetStatus() == :Ready)
			File.new(outputFilePath, 'wb') do |output|
				output.write(report.GetReport())
			end
		end
	end
end
Test::Unit::UI::Console::TestRunner.run(TestClient)
