# "THE BEER-WARE LICENSE" 
# As long as you retain this notice you can do whatever you want with this 
# stuff. If you meet an employee from Windward some day, and you think this
# stuff is worth it, you can buy them a beer in return. Windward Studios

require "uri"
require "net/http"
require "rexml/document"
require "base64"
require_relative "datasource"
include REXML

class XmlDataSource < DataSource
	@data
	@uri
	@schemaData
	@schemaUri
	
	def initialize(data = nil, uri = nil)
		@data = data
		@uri = uri
	end
	
	def SetSchema(data = nil, uri = nil)
		@schemaData = data
		@schemaUri = uri
	end
	
	def GetXml(name)
		File.read(@data)
		element = Element.new "Datasource"
		e = element.add_element "Name"
		e.add_text name.to_s
		e = element.add_element "Type"
		e.add_text "xml"
		
		#puts Base64.encode64(File.read(@data))
		if(@data != nil)
			e = element.add_element "Data"
			e.add_text Base64.encode64(File.binread(@data))
		elsif(@uri != nil)
			e = element.add_element "Uri"
			e.add_text @uri.to_s
		end
		
		if(@schemaData != nil)
			puts File.read(@schemaData)
			e = element.add_element "SchemaData"
			e.add_text Base64.encode64(File.Read(@schemaData))
		elsif(@schemaUri != nil)
			e = element.add_element "SchemaUri"
			e.add_text @schemaUri.to_s
		end

		if(@variables != nil && @variables.length > 0)
			element.add_element(GetVariablesXml())
		end
		return element
	end
end