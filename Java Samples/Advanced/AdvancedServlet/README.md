# Overview
This is a basic example on how to use the [Windward Reports Java Engine](https://www.windwardstudios.com/version/version-downloads) in a web application. This example is a pure servlet implemtnation (no JSPs or EJBs).

# Basic Information
The instructions are for Windows systems but the setup process is similar for any other system which Tomcat supports.

This example has 2 front end sites:
1. Static. Looking at information on a specific employee. A single button to generate a report for that individual.
2. Dynamic. Choose which leave request number you want the report to be on and the output format.

### Requirements
- [Java SE JDK](https://www.oracle.com/java/technologies/downloads/) 32 or 64 bit will work
- [Tomcat 9](https://tomcat.apache.org/download-90.cgi)
- [IntelliJ](https://www.jetbrains.com/idea/)
- [Smart Tomcat](https://plugins.jetbrains.com/plugin/9492-smart-tomcat) Intellij plugin
- [Valid License Key](https://www.windwardstudios.com/trial/download)

# Setup

### Set the **JAVA_HOME** path environment variable.
1. Click on the bottom search bar in the task bar.
2. Type "environment variables" and click on "Edit the System Environment Variables"
3. Under "System Variables" look for the **JAVA_HOME** variable.
    - If it is not there, click "New...", then type **JAVA_HOME** for Variable Name and the path of your jdk installation for the Variable Value
    - If it already exists, click "Edit..." and ensure that it points to the path of your jdk installation.

# Tutorial

### Verify Tomcat 
1. Download [Tomcat 9](https://tomcat.apache.org/download-90.cgi) and using the __Core Zip__ distribution
2. Unzip the file to your `C:` Directory as `tomcat9`.
3. Navigate to the `C:/tomcat9/bin` and run the `startup.bat` file.
4. Navigate to [http://localhost:8080/](http://localhost:8080/) and verify tomcat runs successfully
![apache homepage](./readme_images/apache_home_screen.JPG).
5. From the `C:/tomcat9/bin` folder run the `shutdown.bat` file.

### Set up the Project
1. Open the `ServletExample.ipr` project using IntelliJ
2. Navigate to the project structure ![project structure](./readme_images/project_structure.JPG)
3. Verify the correct Java SDK is selected and the **Servlet API** from the Tomcat zip are pointing to the correct location. ![dependencies](./readme_images/required_jars.JPG)

### Setting the License Key
1. From inside the `AdvancedServlet` folder navigate to the `WEB-INF` folder and open the `WindwardReport.properties` file.
2. Replace `[[LICENSE]]` with your license key

### Verify Windward Maven Repo
1. Right-click on **AdvancedServlet** in the Project Window ![project maven](./readme_images/project_maven.JPG)
2. Then on the bottom, go to Maven -> Reload project

### Configuring and Running the Smart Tomcat Plugin
1. The Smart Tomcat plugin can be installed by using the link in the requirements section or follow the Intellij docs on [managing plugins](https://www.jetbrains.com/help/idea/managing-plugins.html)
2. Select **Add Configuration...** on the top of the IntelliJ window ![add configuration](./readme_images/add_configuration.JPG)
3. Click the **+** sign on the left corner to add a new configuration and select Smart Tomcat which display the configurations ![smart tomcat config](./readme_images/smart_tomcat_config.JPG)
4. Set the **Name** as **JavaServletExample**
5. If **Tomcat Server** is not set, then click the "..." and select the Tomcat folder at `C:/tomcat9`
6. For **Deployment Directory**, click on the folder icon in the text box and click **OK**
7. Change the **Context Path** to **JavaServletExample**
8. Click **Apply** and then **OK**
9. The **Add Configuration...** will now be set to **JavaServletExample** and then click the play button to run the configuration

### Testing the Sample
1. Navigate to [http://localhost:8080/JavaServletExample](http://localhost:8080/JavaServletExample)
2. Scroll down to "Testing the sample" and click on the first sample
3. Click "Create Letter" to run and open the report
4. Return to [http://localhost:8080/JavaServletExample](http://localhost:8080/JavaServletExample)
5. Scroll down the "Testing the sample" and click on the second example
6. Choose a name from the dropdown and choose a report output
7. Click "Run Report" to run and open the report.