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

// Get the version number and make sure the service is up and running.

require_once("sampleinc.php");

try {
	$version = Report::getVersion($serverUri);
	echo "service version " . $version["ServiceVersion"] . ", engine version " . $version["EngineVersion"] . "\n";
	echo "done\n";
}
catch (ReportException $e) {
	showError($e);
}

?>
