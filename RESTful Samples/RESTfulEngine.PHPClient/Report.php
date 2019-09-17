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

require_once("ReportException.php");
require_once("JsonClient.php");

/** The base class for different report types.
 */
abstract class Report {

	/** Returns a version of the service and underlying engine.
	 */
	public static function getVersion($uri)
	{
		$result = JsonClient::get(Report::makeUri($uri, "v1/version"));
		if ($result["status"] != 200)
			throw new ReportException($result["result"]);
		return json_decode($result["result"], true);
	}

	/** Constructs a new instance of the Report class. Use one of the descendants to generate
	 * a desired output.
	 */
	public function __construct($uri, $template)
	{
		$this->state["base_uri"] = $uri;
		$this->state["template"] = $template;
		$this->resetState();
	}
	
	/** Processes the template. This does not apply any data sources, so basically you'll get back
	 * the same template.
	 * Call getReport() to obtain the output.
	 * If 'async' is true, the method returns immediately. Use getStatus() then and when the report
	 * is ready, call getReport() to obtain the output.
	 */
	public function process($async = false)
	{
		$data = $this->prepareForProcess();
		$this->processRequest($data, $async);
	}

	/** Processes the template. All data sources are applied in order.
	 * Call getReport() to obtain the output.
	 * If 'async' is true, the method returns immediately. Use getStatus() then and when the report
	 * is ready, call getReport() to obtain the output.
	 */
	public function processData($dataSources, $async = false)
	{
		$data = $this->prepareForProcess();
		$this->applyDataSources($data, $dataSources);
		$this->processRequest($data, $async);
	}

	/** Returns the generated output.
	 * If the output not ready yet, NULL is returned.
	 */
	public function getReport()
	{
		if (!$this->state["guid"])
			return $this->state["output"];
		else {
			$result = JsonClient::get(Report::makeUri($this->state["base_uri"], "v1/reports/") . $this->state["guid"]);
			if ($result["status"] == 200) {
				$data = json_decode($result["result"], true);
				return $this->state["output"] = base64_decode($data["Data"]);
			}
		}
		return NULL;
	}

	/** Delete generated output.
	 * This is needed if you do an asynchronous processing. Call delete() after you've obtained
	 * the output with getReport().
	 */
	public function delete()
	{
		$result = JsonClient::delete(Report::makeUri($this->state["base_uri"], "v1/reports/") . $this->state["guid"]);
	}

	/** Asynchronous processing status constants.
	 */
	const STATUS_READY = 0;
	const STATUS_WORKING = 1;
	const STATUS_ERROR = 2;
	const STATUS_NOTFOUND = 3;
	
	/** Gets the status of asynchronous processing. Returns one of the above constants.
	 */
	public function getStatus()
	{
		$result = JsonClient::get(Report::makeUri($this->state["base_uri"], "v1/reports/") . $this->state["guid"] . "/status");
		if ($result["status"] == 200)
			return Report::STATUS_READY;
		else if ($result["status"] == 202)
			return Report::STATUS_WORKING;
		else if ($result["status"] == 500)
			return Report::STATUS_ERROR;
		else
			return Report::STATUS_NOTFOUND;
	}

	/** Different report options.
	 */

	/** Description
	 */
	public function getDescription()
	{
		return $this->state["description"];
	}
	public function setDescription($value)
	{
		$this->state["description"] = $value;
	}

	/** Title
	 */
	public function getTitle()
	{
		return $this->state["title"];
	}
	public function setTitle($value)
	{
		$this->state["title"] = $value;
	}

	/** Subject
	 */
	public function getSubject()
	{
		return $this->state["subject"];
	}
	public function setSubject($value)
	{
		$this->state["subject"] = $value;
	}

	/** Keywords
	 */
	public function getKeywords()
	{
		return $this->state["keywords"];
	}
	public function setKeywords($value)
	{
		$this->state["keywords"] = $value;
	}

	/** Locale, e.g. en_US.
	 * See java.util.Locale for more details.
	 */
	public function getLocale()
	{
		return $this->state["locale"];
	}
	public function setLocale($value)
	{
		$this->state["locale"] = $value;
	}

	/** Report processing timeout. 0 (default) means no timeout.
	 */
	public function getTimeout()
	{
		return $this->state["timeout"];
	}
	public function setTimeout($value)
	{
		$this->state["timeout"] = $value;
	}

	/** Hyphenation options
	 */

	const HYPHENATION_ON = "on";
	const HYPHENATION_OFF = "off";
	const HYPHENATION_TEMPLATE = "template";
	
	public function getHyphenate()
	{
		return $this->state["hyphenate"];
	}
	public function setHyphenate($value)
	{
		$this->state["hyphenate"] = $value;
	}

	/** Whether to track imports. Boolean value.
	 */
	public function getTrackImports()
	{
		return $this->state["trackimports"];
	}
	public function setTrackImports($value)
	{
		$this->state["trackimports"] = $value;
	}

	/** How to deal with unused formats. Boolean value.
	 */
	public function getRemoveUnusedFormats()
	{
		return $this->state["removeunusedformats"];
	}
	public function setRemoveUnusedFormats($value)
	{
		$this->state["removeunusedformats"] = $value;
	}

	/** Metadata options.
	 */
	const METADATA_IFNODATASOURCE = "nodatasource";
	const METADATA_NEVER = "never";
	const METADATA_ALWAYS = "always";
	
	public function getCopyMetadata()
	{
		return $this->state["copymetadata"];
	}
	public function setCopyMetadata($value)
	{
		$this->state["copymetadata"] = $value;
	}

	/** A list of supported output formats.
	 */
	const OUTPUT_FORMAT_CSV  = "csv";
	const OUTPUT_FORMAT_DOCX = "docx";
	const OUTPUT_FORMAT_HTML = "html";
	const OUTPUT_FORMAT_PDF  = "pdf";
	const OUTPUT_FORMAT_PPTX = "pptx";
	const OUTPUT_FORMAT_RTF  = "rtf";
	const OUTPUT_FORMAT_XLSX = "xlsx";

	/** Internal implementation.
	 */

	protected abstract function getOutputFormat();
	
	private static function makeUri($baseUri, $suffix)
	{
		$uri = $baseUri;
		if ($uri[strlen($uri) - 1] != "/")
			$uri .= "/";
		return $uri . $suffix;
	}

	private function resetState()
	{
		$this->state["output"] = NULL;
		$this->state["guid"] = NULL;

		$this->state["description"] = NULL;
		$this->state["title"] = NULL;
		$this->state["subject"] = NULL;
		$this->state["keywords"] = NULL;
		$this->state["locale"] = NULL;
		$this->state["timeout"] = NULL;
		$this->state["hyphenate"] = NULL;
		$this->state["trackimports"] = NULL;
		$this->state["removeunusedformats"] = NULL;
		$this->state["copymetadata"] = NULL;
	}

	private function prepareForProcess()
	{
		$this->resetState();
		$data = array();
		$data["Data"] = base64_encode($this->state["template"]);
		$data["OutputFormat"] = $this->getOutputFormat();
		return $data;
	}

	private function processRequest($data, $async)
	{
		$this->setOption($this->state, "description", $data, "Description");
		$this->setOption($this->state, "title", $data, "Title");
		$this->setOption($this->state, "subject", $data, "Subject");
		$this->setOption($this->state, "keywords", $data, "Keywords");
		$this->setOption($this->state, "locale", $data, "Locale");
		$this->setOption($this->state, "timeout", $data, "Timeout");
		$this->setOption($this->state, "hyphenate", $data, "Hyphenate");
		$this->setOption($this->state, "trackimports", $data, "TrackImports");
		$this->setOption($this->state, "removeunusedformats", $data, "RemoveUnusedFormats");
		$this->setOption($this->state, "copymetadata", $data, "CopyMetadata");

		if ($async)
			$data["Async"] = true;

		$json = json_encode($data);
		
		$result = JsonClient::post(Report::makeUri($this->state["base_uri"], "v1/reports"), $json);
		if ($result["status"] != 200)
			throw new ReportException($result["result"]);
		$data = json_decode($result["result"], true);
		if ($async)
			$this->state["guid"] = $data["Guid"];
		else
			$this->state["output"] = base64_decode($data["Data"]);
	}

	private function setOption($source, $sourceName, $dest, $destName)
	{
		if ($source[$sourceName])
			$dest[$destName] = $source[$sourceName];
	}

	private function applyDataSources(&$data, $dataSources)
	{
		$data["Datasources"] = array();
		foreach ($dataSources as $key => $value) {
			$data["Datasources"][] = $value->getObject($key);
		}
	}

	private $state = array();
} // class

?>
