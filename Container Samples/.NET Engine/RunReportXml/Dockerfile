# Grab the SDK image to compile for the CLR
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /app

# Copy the Necessary Files over to the image
COPY RunReportXml.sln .
RUN mkdir RunReportXml
RUN mkdir Samples
COPY RunReportXml/RunReportXml.csproj RunReportXml/App.config RunReportXml/RunReportXml.cs ./RunReportXml/
COPY Samples ./Samples
WORKDIR /app/RunReportXml
RUN mkdir Properties
COPY RunReportXml/Properties/AssemblyInfo.cs Properties

# Restore the nuget packages to ensure that all dependencies are met
WORKDIR /app
RUN nuget restore

# Build the application
WORKDIR /app/RunReportXml
RUN msbuild RunReportXml.csproj /property:Configuration=Release

# Grab the .NET Framework Runtime for 4.8
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8 AS runtime

# Copy over the built project to the image
WORKDIR /app
RUN mkdir Release
COPY --from=build /app/RunReportXml/bin/Release ./Release
RUN mkdir Samples
COPY --from=build /app/Samples ./Samples

# Set the Entry point of the container to the application
WORKDIR /app/Release
ENTRYPOINT ["RunReportXml.exe"]