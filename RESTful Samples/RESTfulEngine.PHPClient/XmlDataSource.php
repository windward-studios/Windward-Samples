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

require_once("DataSource.php");

/** An XML data source implementation.
 */
class XmlDataSource extends DataSource {

	/** Creates a new instance of the data source initialized with a contents of a file.
	 */
	public static function newInstance($data)
	{
		$ds = new XmlDataSource();
		$ds->setData($data);
		return $ds;
	}

	/** Creates a new instance of the data source initialized with the URI.
	 * The contents will be fetched by the server.
	 */
	public static function newInstanceWithUri($uri)
	{
		$ds = new XmlDataSource();
		$ds->setUri($uri);
		return $ds;
	}

	/** XSD schema options. If both are set, the $data version has precedence.
	 */
	public function setSchema($data)
	{
		$this->state["schemadata"] = $data;
	}
	public function setSchemaWithUri($uri)
	{
		$this->state["schemauri"] = $uri;
	}

	public function getObject($name)
	{
		$data["Name"] = $name;
		$data["Type"] = "xml";
		
		if ($this->state["data"])
			$data["Data"] = base64_encode($this->state["data"]);
		else if ($this->state["uri"])
			$data["Uri"] = $this->state["uri"];

		if ($this->state["schemadata"])
			$data["SchemaData"] = base64_encode($this->state["schemadata"]);
		else if ($this->state["schemauri"])
			$data["SchemaUri"] = $this->state["schemauri"];
		
		$this->applyVariables($data);
		
		return $data;
	}
	
	private function setData($data)
	{
		$this->state["data"] = $data;
	}

	private function setUri($uri)
	{
		$this->state["uri"] = $uri;
	}

	protected function __construct()
	{
		parent::__construct();

		$this->state["data"] = NULL;
		$this->state["uri"] = NULL;
		$this->state["schemadata"] = NULL;
		$this->state["schemauri"] = NULL;
	}
}

?>
