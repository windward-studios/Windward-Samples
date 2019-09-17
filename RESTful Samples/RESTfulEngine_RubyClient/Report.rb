# "THE BEER-WARE LICENSE" 
# As long as you retain this notice you can do whatever you want with this 
# stuff. If you meet an employee from Windward some day, and you think this
# stuff is worth it, you can buy them a beer in return. Windward Studios

require "uri"
require "net/http"
require "rexml/document"
require "base64"
#require 'nokogiri'
require_relative 'Client'
require_relative 'Version'
require 'json/pure'
include REXML

class Report
	@baseUri
	@template
	@report
	@guid
	
	attr_accessor :Description
	attr_accessor :Title
	attr_accessor :Subject
	attr_accessor :Keywords
	attr_accessor :Locale
	attr_accessor :Timeout
	
	attr_accessor :Hyphenate
	attr_accessor :TrackImports
	attr_accessor :RemoveUnusedFormats
	attr_accessor :CopyMetadata
	
	attr_reader :OutputFormat
	
	def initialize(baseUri, template, report = nil)
		ctor(baseUri)
		@template = template
		@report = report
	end	
	def ctor(baseUri)
		url = baseUri.to_s
		if !url.end_with? "/"
			url.concat("/")
		end
		@baseUri = URI(url)
		@Timeout = 0
		@Hyphenate = :template
		@TrackImports = false
		@RemoveUnusedFormats = true
		@CopyMetadata = :nodatasource
	end
	
	def self.GetVersion(baseUri)
		uri = URI.join(baseUri, "v1/version")
		response = Client.get(uri)
		status = response.message
		body = response.body
		p_body = JSON.parse(body)
		if(status == 'OK')			
			ev = p_body["EngineVersion"].to_s
			sv = p_body["ServiceVersion"].to_s
			v = Version.new(sv, ev)
		else
			return nil
		end
		return v
	end
	
	def ProcessDataSources(dataSources)
		xml = CreateXmlDocument()
		ApplyDataSources xml, dataSources
		ProcessXml xml
	end
	
	def Process()
		xml = CreateXmlDocument()
		ProcessXml xml
	end
	
	def ProcessXml(xml)
		SetReportOption(xml, "Description", @Description)
		SetReportOption(xml, "Title", @Title)
		SetReportOption(xml, "Subject", @Subject)
		SetReportOption(xml, "Keywords", @Keywords)
		SetReportOption(xml, "Description", @Description)
		SetReportOption(xml, "Locale", @Locale)
		
		root = xml.root
		e = root.add_element "Timeout"
		e.add_text @Timeout.to_s
		
		e = root.add_element "Hyphenate"
		e.add_text @Hyphenate.to_s
		
		if(@TrackImports)
			_track_imports = "true"
		else
			_track_imports = "false"
		end
		
		e = root.add_element "TrackImports"
		e.add_text _track_imports
		
		if(@RemoveUnusedFormats)
			_remove_form = "true"
		else
			_remove_form = "false"
		end
		
		e = root.add_element "RemoveUnusedFormats"
		e.add_text _remove_form
		
		e = root.add_element "CopyMetadata"
		e.add_text @CopyMetadata.to_s
		
		if(@report == nil)
			e = root.add_element "Async"
			e.add_text "true" 
		end
		result = Client.post(URI.join(@baseUri, "v1/reports"), xml)
		status = result.message
		body = result.body
		if(status == 'OK')
			if(@report != nil)
				ReadReport(body)
			else
				ReadGuid(body)
			end
		else
			raise body.to_s
		end
	end
	
	def GetReport
		uri = URI.join(@baseUri, "v1/reports")
		uri = URI.join(uri, @guid)
		
		result = Client.Get(uri)
		status = result.message
		body = result.body
		if(status == 'OK')
			return Base64.decode64(body.root.get_elements("Data")[0].get_text.value)
		end
		return null
	end
	
	def Delete
		uri = URI.join(@baseUri, v1/reports)
		uri = URI.join(uri, @guid)
		result = Client.Delete(uri)
	end
	
	def GetStatus
		uri = URI.join(@baseUri, "v1/reports/")
		uri = URI.join(uri, @guid)
		uri = URI.join(uri, "/status")
		result = Client.get(uri)
		status = result.code
		
		case status
			when 200
				return :Ready
			when 202
				return :Working
			when 500
				return :Error
			else
				return :NotFound
		end
	end
	
	def ReadReport result
		p_result = JSON.parse(result)
		data = Base64.decode64(p_result["Data"])
		@report.write(data)
		#File.open(@report, 'wb') do |output|
		#	output.puts data
		#end
	end
	
	def ReadGuid body
		p_body = JSON.parse(body)
		@guid = body["Data"][0]
	end
	
	def SetReportOption xml, name, option
		if(option != nil)
			e = xml.root.add_element name
			e.add_text option.to_s
		end
	end
		
	def CreateXmlDocument
		bytes = File.binread(@template)
		encodedTemplate = Base64.encode64(bytes)
		xml = Document.new
		e = xml.add_element "Template"
		e1 = e.add_element "Data"
		e1.add_text encodedTemplate
		e1 = e.add_element "OutputFormat"
		e1.add_text @OutputFormat.to_s
		return xml
	end
		
	def ApplyDataSources xml, dataSources
		if(dataSources.length > 0)
			xmlDatasources = xml.root.add_element "Datasources"
			dataSources.each do |key, value|
				e = xmlDatasources.add_element(value.GetXml(key).root)
			end
		end
	end
end