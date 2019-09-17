require_relative 'report'
class ReportPptx < Report
	def initialize uri, template, report = nil
		@OutputFormat = :pptx
		super
	end
end