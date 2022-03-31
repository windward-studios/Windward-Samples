### Overview
These files are used to create a command-line `.class` file which is the equivalent of the sample located in the `/test` folder of the [Windward Reports Java Engine](https://www.windwardstudios.com/version/version-downloads) zip file.

### Requirements
- [Windward Reports Java Engine](https://www.windwardstudios.com/version/version-downloads)
- [IntelliJ IDEA](https://www.jetbrains.com/idea/download/#section=windows)
- [Minimum Java JDK v1.4](https://www.oracle.com/java/technologies/downloads/)

### Getting Started
1. Download the [Windward Reports Java Engine](https://www.windwardstudios.com/version/version-downloads) 
and extract the files from the zip to a location of your choice to obtain the required jar files.

2. Open the `RunReport.ipr` file using **IntelliJ IDEA** 
and verify the correct version of Java being used. This is done by clicking the cog/settings icon (top right of page), project structure, Module SDK 
![IntelliJ](/doc_images/project_structure.JPG)

3. Modify the path to the Windward Jar files by clicking on **Windward Jars** -> `+` -> select the folder that contains the jars.
![Windward Jars](/doc_images/windward_jars.JPG)


> :question: **If build error unable to find Reports:** open the `pom.xml` file and verify the value of **windward_jars** in `build.xml` is the path to the Windward jars from step 1.


5. Add your license key in the `WindwardReports.properties` file by replacing `[[license]]` with your license key.

6. Build the project and run the program to output the sample as `Invoice.pdf`
![Build and Run](/doc_images/build_and_run.JPG)

> :question: **Run the program without file automatically opening:** Click __RunReport__ and change to __RunReportWithoutOpen__ which will output as `Invoice2.pdf`