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

require_once("Report.php");

/** Generates a report in the PDF format.
 */
class ReportPdf extends Report {

	protected function getOutputFormat()
	{
		return Report::OUTPUT_FORMAT_PDF;
	}
}

?>
