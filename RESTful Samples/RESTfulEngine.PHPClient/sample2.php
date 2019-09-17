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

// Process a template with an XML data source.

require_once("sampleinc.php");

try {
	// Construct a data source.
	$dataSources = array();
	$dataSources["MANF_DATA_2009"] = XmlDataSource::newInstance(readFromFile("../SampleTemplates/Manufacturing.xml"));

	// Load a template and create a Report instance.
	$template = readFromFile("../SampleTemplates/Manufacturing.docx");
	$report = new ReportPdf($serverUri, $template);
	
	// Process the template.
	$report->processData($dataSources);
	
	// Obtain the output.
	saveToFile("sample2output.pdf", $report->getReport());
	
	echo "generated report in sample2output.pdf\ndone\n";
}
catch (ReportException $e) {
	showError($e);
}

?>
