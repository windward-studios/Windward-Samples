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

/** ADO data source implementation.
 */
class AdoDataSource extends DataSource {

	/** Creates a new instance of the data source. For $className and $connectionString values
	 * consult the ADO.NET data source provider being used.
	 */
	public static function newInstance($className, $connectionString)
	{
		return new AdoDataSource($className, $connectionString);
	}

	/** Returns an object representation of this data source suitable to be sent over to the server.
	 * Normally, this is used by Report classes and is useless for the client.
	 */
	public function getObject($name)
	{
		$data["Name"] = $name;
		$data["Type"] = "sql";
		$data["ClassName"] = $this->state["classname"];
		$data["ConnectionString"] = $this->state["connectionstring"];

		$this->applyVariables($data);

		return $data;
	}
	
	protected function __construct($className, $connectionString)
	{
		parent::__construct();

		$this->state["classname"] = $className;
		$this->state["connectionstring"] = $connectionString;
	}
}

?>
