Overviewjavelin-wiki-logo.png

This page provides instuction on how to run the RESTful Engine Ruby client.
Prerequisites

Ruby 2.1.3
All gems listed in Gemfile 
This is easily done by running the command "bundle install" within the RESTfulEngine_RubyClient directory
Download the attached RESTfulEngine_RubyClient.zip
Instructions

Copy the RESTfulEngine_RubyClient onto your machine
CD into the RESTfulEngine_RubyClient directory
Run the command "bundle install" to insure you have all necessary ruby gems
If this returns an error you may need to run the command "gem install bundle"
Open the file RESTfulEngine_RubyClient\Test\TestClient.rb and change the URL to match your RESTful Engine URL
CD into the Test directory and run the command "ruby TestClient.rb"