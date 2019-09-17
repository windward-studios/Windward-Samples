# "THE BEER-WARE LICENSE" 
# As long as you retain this notice you can do whatever you want with this 
# stuff. If you meet an employee from Windward some day, and you think this
# stuff is worth it, you can buy them a beer in return. Windward Studios

require "net/http"
require "uri"


class Client
	def self.post(uri, body)
		req = Net::HTTP::Post.new(uri)
		req.body = body.to_s
		req.content_type = 'text/xml'
		return processRequest(req, uri)
	end

	def self.get(uri)
		#req = Net::HTTP::Get.new(uri)
		#return processRequest(req, uri)
		return Net::HTTP.get_response(uri)
	end
	
	def self.delete(uri)
		http = Net::HTTP.new(uri.host, uri.port)
		req = Net::HTTP::Delete.new(uri.path)
		return processRequest(req, uri)
	end
		
	def self.processRequest(req, uri)
		res = Net::HTTP.start(uri.hostname, uri.port) do |http|
			http.request(req)
		end
		return res
	end
	
	def sendRequest(request)
		#TODO
	end
	
	def setRequestBody(request)
		#TODO
	end
	
	def getResponseBody(response)
		#TODO
	end
end