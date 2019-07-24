<?php
/*
 * Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

// Process a template asynchronously. This way you are not forced to wait
// till the report generation completes. You can query for the status of
// the process and retrieve the report when it becomes available.

require_once("sampleinc.php");

try {
	// Construct a data source.
	$dataSources = array();
	$dataSources["MANF_DATA_2009"] = XmlDataSource::newInstance(readFromFile("../SampleTemplates/Manufacturing.xml"));

	// Load a template and create a Report instance.
	$template = readFromFile("../SampleTemplates/Manufacturing.docx");
	$report = new ReportPdf($serverUri, $template);
	
	// Process the template. Note, the second parameter is set to 'true'. The report will be processed
	// asynchronously.
	$report->processData($dataSources, true);
	
	// Wait till the output is generating.
	while ($report->getStatus() == Report::STATUS_WORKING)
		usleep(10000);
	
	// When the output is ready, obtain it.
	if ($report->getStatus() == Report::STATUS_READY) {
		saveToFile("sample5output.pdf", $report->getReport());
		
		// And delete the report.
		$report->delete();
	}
	
	echo "generated report in sample5output.pdf\ndone\n";
}
catch (ReportException $e) {
	showError($e);
}

?>
