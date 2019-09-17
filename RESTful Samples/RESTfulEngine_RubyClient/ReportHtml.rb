require_relative 'report'
class ReportHtml < Report
	def initialize uri, template, report = nil
		@OutputFormat = :html
		super
	end
end