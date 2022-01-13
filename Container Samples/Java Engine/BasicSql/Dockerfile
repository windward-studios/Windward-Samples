# Create an image with JRE 8 to build our app
FROM alpine:latest AS build

# Install the necessary tools
Run apk update
Run apk upgrade
Run apk add --no-cache bash
Run apk add --no-cache openjdk8
Run apk add --no-cache maven

# Make a folder for Windward engine 
# and the data from host machine
RUN mkdir /opt/windward/ 

# Copy all the required files
COPY . /opt/windward/

# Refernce the folder that contains the jar files
#ENV JAVA_ENGINE_HOME=/opt/windward/jars

# This will be the starting directory
WORKDIR /opt/windward

# Build the Custom Application
Run mvn clean compile assembly:single
Run mv ./target/BasicSql-1.0-SNAPSHOT-jar-with-dependencies.jar ./target/BasicSql.jar

# Create the container image
FROM alpine:latest AS runtime
Run apk update
Run apk upgrade
Run apk add --no-cache openjdk8
Run apk add --no-cache bash
WORKDIR /app
Run mkdir Release
Run mkdir Samples

# Copy over our built application
COPY --from=build /opt/windward/WindwardReports.properties ./Samples
COPY --from=build /opt/windward/target/* ./Release
COPY --from=build /opt/windward/data/* ./Samples
WORKDIR ./Release
WORKDIR ../Samples

# Set the entry point to run our app when the container is launched
ENTRYPOINT  java -jar ../Release/BasicSql.jar