# Grab Alpine Linux base image
FROM alpine:latest as base

# Add necessary tools
RUN apk update
RUN apk upgrade
RUN apk add --no-cache bash
RUN apk add --no-cache openjdk8

# Copy the tomcat installation over to the /opt directory, and set as working directory
COPY ./ /opt

# Replace VERSION_NUMBER below to match the version of tomcat that you are using (Ex:apache-tomcat-9.0.56)
WORKDIR /opt/apache-tomcat-VERSION_NUMBER

# Set the container to start the Tomcat Server when launched, and sleep so that the container doesn't exit afterwards
ENTRYPOINT ./bin/catalina.sh start && sleep infinity