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

// Process a template with variables.

require_once("sampleinc.php");

try {
	// Create a list of variables that are used by a template.
	$vars = array();
	$vars[] = new TemplateVariable("Var1", "hi there");
	
	// Construct a data source and set the variables.
	$ds = XmlDataSource::newInstance(readFromFile("../SampleTemplates/Manufacturing.xml"));
	$ds->setVariables($vars);

	// Here we are using the data source without a name. Only one unnamed data source is allowed.
	$dataSources = array("" => $ds);

	// Load a template and create a Report instance.
	$template = readFromFile("../SampleTemplates/Variables.docx");
	$report = new ReportPdf($serverUri, $template);
	
	// Process the template.
	$report->processData($dataSources);
	
	// Obtain the output.
	saveToFile("sample4output.pdf", $report->getReport());
	
	echo "generated report in sample4output.pdf\ndone\n";
}
catch (ReportException $e) {
	showError($e);
}

?>
