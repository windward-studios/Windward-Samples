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

/** The base class for all data source.
 */
abstract class DataSource {

	public function getVariables()
	{
		return $this->state["variables"];
	}
	public function setVariables($value)
	{
		$this->state["variables"] = $value;
	}

	public abstract function getObject($name);
	
	protected function __construct()
	{
		$this->state["variables"] = NULL;
	}
	
	protected function applyVariables(&$data)
	{
		$vars = $this->getVariables();
		if (!$vars || !count($vars))
			return;

		$data["Variables"] = array();
		foreach ($vars as $var) {
			$v["Name"] = $var->getName();
			$v["Value"] = $var->getValue();
			$data["Variables"][] = $v;
		}
	}
}

?>
