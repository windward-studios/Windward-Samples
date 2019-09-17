require_relative 'report'
class ReportRtf < Report
	def initialize uri, template, report = nil
		@OutputFormat = :rtf
		super
	end
end