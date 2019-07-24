# Copyright (c) 2015 Windward Studios
#
#
# Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
# to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
#  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


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