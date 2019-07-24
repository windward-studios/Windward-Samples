# restfulclient-typescript
Windward's TypeScript Client for the RESTful Engine.

## How to use this module
#### Pre-requisites
First, let's get some pre-requisites out of the way. This module was designed
to run in Node, but can also be used in the browser.

It utilizes the new [fetch API](https://jakearchibald.com/2015/thats-so-fetch/)
and as such, you'll need a fetch polyfill.  We suggest
[isomorphic-fetch](https://github.com/matthew-andrews/isomorphic-fetch)
but you can also use [node-fetch](https://github.com/bitinn/node-fetch) or
[GitHub's fetch](https://github.com/github/fetch) depending on your needs.

These fetch polyfills also require a Promise polyfill depending on your environment
requirements.  For this, [es6-polyfill](https://github.com/stefanpenner/es6-promise).

Finally, this package uses node's buffer to read and write file data.  When running
from the browser, this isn't available natively unless you pick up a [buffer
for browser](https://github.com/feross/buffer) -- although if you are making calls
from a browser, you are most likely going to be using the URI APIs.

#### Tests
A great way to get up and running is to see how the tests work.  If this is your style,
head over to [test/test.ts](test/test.ts)

If you downloaded this repository, you can run these with `npm test` (after
running `npm install` of course)

#### Get running
To get the library, you can either download the dist directory from here or install with

```
npm install restfulclieint
```

If you installed with npm, you are set to import directly from node_modules.
You can follow one of 2 import styles:

```typescript
import {
    getVersion, createReport, OutputFormat, XmlDataSource, AdoDataSource,
    JsonDataSource, ODataDataSource, TemplateVariable, DataSet, Status, Version
} from 'restfulclient';
```

OR

```typescript
import * as restfulclient from 'restfulclient';
```

#### Reading files from file system
When running reports, either the templates are already on the server in which case
you just have to tell restfulclient where they are.  Otherwise you need to load up a
file to send to the server.  If you need to load a file, here's how to do it:

```typescript
import * as fs from 'fs';
var template = fs.readFileSync("path/to/template");
```

This puts a [`Buffer`](https://nodejs.org/api/buffer.html) in template using Node's
file system library.  You'll have to devise other means of getting a buffer if you are
running in the browser.

#### Creating a report
Now, we'll run a report, assuming you have imported everything using option 1 above.
Use the `createReport()` method which takes a template and an OutputFormat as well as
the address to your RESTful Engine server.  Other options can be specified optionally
on the returned `Report` object.

```typescript
var report = createReport("http://localhost:49731/", OutputFormat.PDF, template);
report.hyphenate = Hyphenation.Off;
```

#### Adding DataSources
To add a DataSource, simply push it to the report's datasources array:

```typescript
report.dataSources.push(
    new AdoDataSource("MSSQL", "System.Data.SqlClient",
        "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo"
    )
);
```

DataSets are done in the same way:

```typescript
var dataset; // contains an rdlx file in a Buffer previously read by the file system
report.dataSets.push(new DataSet(dataset));
```

#### Adding a variable
Adding a variable is just as easy -- you push variables onto DataSources rather
than Templates though:

```typescript
var dataSource = new AdoDataSource("MSSQL", "System.Data.SqlClient",
    "Data Source=mssql.windward.net;Initial Catalog=Northwind;User=demo;Password=demo"
);
datasource.variables.push(new TemplateVariable("Var1", "hi there", "text"));
report.dataSources.push(datasource);
```

#### Running
You want to run? Call `process()` which returns a `Promise`.  It only returns a
Promise because of how the fetch API works (although it's a really good idea to
use promises rather than blocking the main thread for a server call).

```typescript
report.process();
```

You can optionally pass dataSources and dataSets to process() rather than adding
them yourself.  These get pushed to report.

```typescript
report.process(dataSources, dataSets);
```

#### Async
The RESTful Engine provides an async API you can take advantage of if you want.
In JavaScript, this isn't as useful given the async features of the language,
but it will help if you'd rather not hold a connection open while waiting for
a potentially large report.

Just call `processAsync()` instead of `process()` -- this simply sends the
request to the server.

You may use a combination of `getStatus()` and `getReport()` to retrieve the report
(both of these return a `Promise`) or you may call `getReportWhenReady()` which
will poll the server at a period specified in its arguments.  It returns a `Promise`
that eventually resolves to a `Buffer` contianing your output (it'll reject if any
server errors occur)
