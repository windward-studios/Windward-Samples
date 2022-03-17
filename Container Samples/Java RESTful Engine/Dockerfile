FROM openjdk:8 as licenseBuild

# copy files over to image
COPY ./ /opt
WORKDIR /opt/apache-tomcat-9/webapps

# replace the windward properties file from ROOT.war
RUN mkdir -p WEB-INF/classes/ \
    && mv ../../WindwardReports.properties ./WEB-INF/classes/WindwardReports.properties \
    && jar -uvf ROOT.war WEB-INF/classes/WindwardReports.properties \
    && rm -rf WEB-INF

# Grab Alpine Linux base image
FROM alpine:latest as base

# Add necessary tools
RUN apk update
RUN apk upgrade
RUN apk add --no-cache openjdk8

# Copy the tomcat installation over to the /opt directory, and set as working directory
WORKDIR /opt
COPY --from=licenseBuild /opt .

# change permissions to write to write to folders and ensure able to run catalina script
RUN chmod a+rwx apache-tomcat-9/webapps apache-tomcat-9/logs \
    && chmod a+x apache-tomcat-9/bin/catalina.sh

# must be run from the webapps directory
WORKDIR /opt/apache-tomcat-9/webapps
ENTRYPOINT [ "../bin/catalina.sh", "run" ]