
class Version
	attr_accessor :ServiceVersion
	attr_accessor :EngineVersion
	def initialize(sv, ev)
		@ServiceVersion = sv
		@EngineVersion = ev
	end
end