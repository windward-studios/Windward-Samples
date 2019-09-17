# "THE BEER-WARE LICENSE" 
# As long as you retain this notice you can do whatever you want with this 
# stuff. If you meet an employee from Windward some day, and you think this
# stuff is worth it, you can buy them a beer in return. Windward Studios

require_relative 'report'
class ReportXlsx < Report
	def initialize uri, template, report = nil
		@OutputFormat = :xlsx
		super
	end
end