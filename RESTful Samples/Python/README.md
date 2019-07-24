How To Use This Module
======================
*More documentation can be found [On PyPI](http://pythonhosted.org/restfulengine/)*

### 1. Import the module:
```
import restfulengine
```
*OR*
```
from restfulengine import *
```
### 2. Create a Report 
Create a [Report](http://pythonhosted.org/restfulengine/#restfulengine.Report)
either by using its constructor or calling
[create_report()](http://pythonhosted.org/restfulengine/#restfulengine.create_report).
You must specify a service URL, an output format
and a template file (or URI). If you don't specify an output file,
you are enabling asynchronous operation and later, must call
[Report.get_report()](http://pythonhosted.org/restfulengine/#restfulengine.Report.get_report)
to retrieve the output:
```
report = restfulengine.create_report(
    base_uri,
    OutputFormat.pdf,
    template
)
```
### 3. Set up DataSources 
Set up [DataSource](http://pythonhosted.org/restfulengine/#restfulengine.DataSource)
objects and add them to a list.
[XmlDataSource](http://pythonhosted.org/restfulengine/#restfulengine.XmlDataSource)
objects or [AdoDataSource](http://pythonhosted.org/restfulengine/#restfulengine.AdoDataSource) objects:
```
data_sources = [
    AdoDataSource(
        "MSSQL"
        "System.Data.SqlClient",
        "Data Source=mssql.windward.net;"
        "Initial Catalog=Northwind;"
        "User=demo;Password=demo")
]
```
### 4. Process the report
Call [Report.process()](http://pythonhosted.org/restfulengine/#restfulengine.Report.process)
with your data sources to send the request:
```
report.process(data_sources)
```
###5. Retrieve the report
(for async reports) call
[Report.get_status()](http://pythonhosted.org/restfulengine/#restfulengine.Report.get_status)
to poll the server so you know when the report is done.  Then call
[Report.get_report()](http://pythonhosted.org/restfulengine/#restfulengine.Report.get_report)
to retrieve the output.  Finally, call delete
when you are done with the report to delete it from the server:
```
while report.get_status() == Status.working:
    time.sleep(100)
if report.get_status() == Status.ready:
    output = report.get_report()
    report.delete()
```
