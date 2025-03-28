# Grab Tomcat image
FROM tomcat:9.0.71-jdk8-corretto

# Let Docker know that Tomcat will be exposing port 8888
# Do not forget to map out to something useful (-p 8888:8080)
EXPOSE 8888

# Copy the Engine files inside the container
# You can get these files here: https://www.windwardstudios.com/version/version-downloads
COPY App_Data /usr/local/tomcat/webapps/
# Do not forget to tweak to the version you are downloading
COPY JavaRESTfulEngine-22.4.2.2.pom /usr/local/tomcat/webapps/
COPY ROOT.war /usr/local/tomcat/webapps/

# Change the workdir to where Tomcat is installed
WORKDIR /usr/local/tomcat
# Set the container to start the Tomcat Server when launched, and sleep so that the container doesn't exit afterwards
ENTRYPOINT ./bin/catalina.sh start && sleep infinity
