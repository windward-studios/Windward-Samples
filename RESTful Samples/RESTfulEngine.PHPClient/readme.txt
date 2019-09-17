RESTful Engine PHP Client

Overview

This is a PHP client for the Windward RESTful reporting engine.

Prerequisites

1. A working Windward RESTful reporting engine running on a server.
2. The requirement is a working copy of PHP with the libcurl support (which is normally on by default). Go to http://php.net/ for more information on obtaining, installing, and configuring PHP.
3. Download the client source code: PHPClient.zip

Instructions

1. Unpack the PHPClient.zip archive.
	The archive contains two sub-folders: RESTfulEngine.PHPClient and SampleTemplates
2. Edit sampleinc.php and update the $serverUri variable as appropriate for your configuration.
3. Start up a command prompt window.
4. Navigate to the RESTfulEngine.PHPClient folder and execute the 'runallsamples.cmd' script.
	If you are on a Unix system, then run something like this
	$ for f in sample?.php; do php $f &; done
5. Examine the generated .pdf reports.
	In case of an error, the appropriate message is printed to the console.
