/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package net.windward.servletexample;

import javax.servlet.ServletException;

/**
 * ServletException -- specifically a problem with a parameter
 */
public class ParameterException extends ServletException {
    public ParameterException(String message) {
        super(message);
    }
}
