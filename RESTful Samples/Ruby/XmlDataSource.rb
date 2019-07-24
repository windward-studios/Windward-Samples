# Copyright (c) 2015 Windward Studios
#
#
# Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
# to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
#  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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

		if(@Variables != nil && @Variables.Count > 0)
			element.add_element(GetVariablesXml())
		end
		return element
	end
end