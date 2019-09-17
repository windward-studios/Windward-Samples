# "THE BEER-WARE LICENSE" 
# As long as you retain this notice you can do whatever you want with this 
# stuff. If you meet an employee from Windward some day, and you think this
# stuff is worth it, you can buy them a beer in return. Windward Studios


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
		if(@variables != nil && @variables.length > 0)
			element.add_element(GetVariablesXml())
		end
		return element
	end	
end	