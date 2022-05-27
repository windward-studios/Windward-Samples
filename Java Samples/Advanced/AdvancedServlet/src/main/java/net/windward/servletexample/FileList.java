/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package net.windward.servletexample;

import javax.servlet.http.HttpSessionBindingEvent;
import javax.servlet.http.HttpSessionBindingListener;
import java.io.File;
import java.io.Serializable;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;

/**
 * This stores the list of images a session created.
 *
 * @author David Thielen
 * @version 1.0  March 27, 2003
 */

public class FileList implements HttpSessionBindingListener, Serializable {
	
	private List files = new LinkedList();
	
	public void addFile( String file ) {
		
		files.add( file );
	}
	
	public void valueBound(HttpSessionBindingEvent event) {
		// nothing to do
	}
	
	// Delete all the files. This is called when the session is ended because it first
	// unbinds all attributes. This is supposed to be called when the server engine
	// stops or reloads - but Tomcat 4.1.x doesn't do that.
	public void valueUnbound(HttpSessionBindingEvent event) {
		
		for (Iterator it=files.iterator(); it.hasNext(); ) {
			String file = (String) it.next();
			new File( file ).delete();
		}
	}
}

