# "THE BEER-WARE LICENSE" 
# As long as you retain this notice you can do whatever you want with this 
# stuff. If you meet an employee from Windward some day, and you think this
# stuff is worth it, you can buy them a beer in return. Windward Studios

require 'rexml/document'
include REXML

class DataSource
	attr_accessor :variables
	
	def GetVariablesXml()
		element = Element.new "Variables"
		@variables.each do |variable|
			e = element.add_element "Variable"
			el = e.add_element "Name"
			el.add_text variable.Name
			el = e.add_element "Value"
			el.add_text variable.Value
		end
		puts element.to_s
		return element
	end
end
	