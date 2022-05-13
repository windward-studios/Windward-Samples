# Overview
This is a basic example on how to use the [Windward Reports Java Engine](https://www.windwardstudios.com/version/version-downloads) in a web application. This example is a pure servlet implemtnation (no JSPs or EJBs).

# Basic Information
The instructions are for Windows systems but the setup process is similar for any other system which Tomcat supports.

This example has 2 front end sites:
1. Static. Looking at information on a specific employee. A single button to generate a report for that individual.
2. Dynamic. Choose which leave request number you want the report to be on and the output format.

### Requirements
- [Windward Reports Java Engine](https://www.windwardstudios.com/version/version-downloads)
- [Java SE JDK](https://www.oracle.com/java/technologies/downloads/) 32 or 64 bit will work
- [Tomcat 9](https://tomcat.apache.org/download-90.cgi)
- [IntelliJ](https://www.jetbrains.com/idea/)
- [Apache Ant](https://ant.apache.org/manual/install.html)
- Valid License Key

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


### Download the Windward Reports Java Engine
1. Visit the [Windward Download Page](https://www.windwardstudios.com/version/version-downloads) and download the **Windward Reports Java Engine**.
2. Unzip the folder in the `C:` directory as `jars`.

### Set up the Project
1. Open the `ServletExample.ipr` project using IntelliJ
2. Navigate to the project structure ![project structure](./readme_images/project_structure.JPG)
3. Verify the correct Java SDK is selected and both **Windward Jars** from the [Windward Reports Java Engine](https://www.windwardstudios.com/version/version-downloads) download and the **Servlet API** from the Tomcat zip are pointing to the correct location. ![dependencies](./readme_images/required_jars.JPG)

### Setting the License Key
1. From inside the `AdvancedServlet` folder navigate to the `WEB-INF` folder and open the `WindwardReport.properties` file.
2. Replace `[[LICENSE]]` with your license key

### Running the Servlet Example
1. Copy the **AdvancedServlet** folder and paste inside the `C:tomcat9/webapps`  folder.
2. Rename the **AdvancedServlet** folder as **JavaServletExample**
3. From inside the **JavaServletExample** folder run the command `ant compile` (ant is shipped with IntelliJ)
4. Navigate to `C:tomcat9/bin` and run the `startup.bat` file

### Testing the Sample
1. Navigate to [http://localhost:8080/JavaServletExample](http://localhost:8080/JavaServletExample)`
2. Scroll down to "Testing the sample" and click on the first sample
3. Click "Create Letter" to run and open the report
4. Return to [http://localhost:8080/JavaServletExample](http://localhost:8080/JavaServletExample)
5. Scroll down the "Testing the sample" and click on the second example
6. Choose a name from the dropdown and choose a report output
7. Click "Run Report" to run and open the report.

#### Notes
- The source code calling the Windward Java Engine can be found at `C:\tomcat9\webapps\JavaServletExample\src`
- To build from the source either
  - run the `build.bat` in `c:\tomcat7\webapps\JavaServletExample\src\com\windwardreports`
  - Open the AdvancedServlet.ipr in IntelliJ and build the project using the UI.