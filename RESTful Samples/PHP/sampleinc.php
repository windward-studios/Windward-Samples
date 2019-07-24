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
 
/** An include file for the samples.
 */

/** Edit your php.ini for the following to work.
 * This is required to run the samples from the command line.
 */
dl("php_curl");

require_once("RESTfulEngineClient.php");

// Set this to the base URI where the reporting service is running
$serverUri = "http://localhost:49731";

function readFromFile($name)
{
	$file = file_get_contents($name);
	return $file;
}

function saveToFile($name, $data)
{
	file_put_contents($name, $data);
}

function showError($output)
{
	echo $output . "\n";
}

?>
