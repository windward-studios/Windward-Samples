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

/** The template variable.
 */
class TemplateVariable {

	/** Creates a new instance of the variable with name and initial value.
	 */
	public function __construct($name, $value)
	{
		$this->state["name"] = $name;
		$this->state["value"] = $value;
	}

	/** Returns the name of this variable.
	 */
	public function getName()
	{
		return $this->state["name"];
	}

	/** Returns the value of this variable.
	 */
	public function getValue()
	{
		return $this->state["value"];
	}

	/** Assigns a new value to this variable.
	 */
	public function setValue($value)
	{
		$this->state["value"] = $value;
	}
}

?>
