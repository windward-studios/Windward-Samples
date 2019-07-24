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

require 'rexml/document'
require_relative 'DataSource'
include REXML

class AdoDataSource < DataSource
	@className
	@connectionString
	
	def initialize(className, connectionString)
		@className = className
		@connectionString = connectionString
	end
	
	def GetXml(name)
		element = Element.new "Datasource"
		e = element.add_element "Name" 
		e.add_text name.to_s
		e = element.add_element "Type" 
		e.add_text "sql"
		e = element.add_element "ClassName" 
		e.add_text @className.to_s
		e = element.add_element "ConnectionString" 
		e.add_text @connectionString.to_s
		if @Variables != nil && @Variables.length > 0
			element.add_element(GetVariablesXml)
		end
		return element
	end	
end	