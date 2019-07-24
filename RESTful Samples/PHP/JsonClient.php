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

/** A helper to communicate with the server.
 */
class JsonClient {

	public static function post($uri, $json)
	{
		return JsonClient::sendRequest($uri, "POST", $json);
	}
	
	public static function get($uri)
	{
		return JsonClient::sendRequest($uri, "GET", NULL);
	}
	
	public static function delete($uri)
	{
		return JsonClient::sendRequest($uri, "DELETE", NULL);
	}
	
	private static function sendRequest($uri, $method, $json)
	{
		$ch = curl_init($uri);
		curl_setopt($ch, CURLOPT_CUSTOMREQUEST, $method);
		$header = array("Accept: application/json");
		if ($json) {
			curl_setopt($ch, CURLOPT_POSTFIELDS, $json);
			array_push($header, "Content-type: application/json");
			array_push($header, "Content-length: " + strlen($json));
		}
		curl_setopt($ch, CURLOPT_HTTPHEADER, $header);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
		$out = curl_exec($ch);
		$status = curl_getinfo($ch, CURLINFO_HTTP_CODE);
		curl_close($ch);
		return array("status" => $status, "result" => $out);
	}
}

?>
