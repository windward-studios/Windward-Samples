# Grab Alpine Linux base image
FROM alpine:latest as base

# Add necessary tools
Run apk update
Run apk upgrade
Run apk add --no-cache bash
Run apk add --no-cache openjdk8

# Copy the tomcat installation over to the /opt directory, and set as working directory
COPY ./ /opt
WORKDIR /opt/apache-tomcat-9.0.56

# Set the container to start the Tomcat Server when launched, and sleep so that the container doesn't exit afterwards
ENTRYPOINT ./bin/catalina.sh start && sleep infinity