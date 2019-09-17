require_relative 'report'
class ReportPdf < Report
	def initialize uri, template, report = nil
		@OutputFormat = :pdf
		super
	end
end