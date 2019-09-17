require_relative 'report'
class ReportDocx < Report
	def initialize uri, template, report = nil
		@OutputFormat = :docx
		super
	end
end