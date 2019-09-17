/*
* Copyright (c) 2016 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/

package client;

import com.sun.org.apache.xerces.internal.impl.dv.util.Base64;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import java.io.IOException;
import java.io.InputStream;
import java.net.URL;

/**
 * Defines a dataset. This is a POD file that you want to pass to the engine for processing.
 */
public class Dataset {
    private InputStream data;
    private URL url;

    /**
     * Creates a new instance of the object.
     *
     * @param data The input stream to read the POD file from.
     */
    public Dataset(InputStream data) {
        this.data = data;
    }

    /**
     * Creates a new instance of the object.
     *
     * @param url The location of the POD file. This location must be accessible from the server.
     */
    public Dataset(URL url) {
        this.url = url;
    }

    Element getXml(Document doc) throws IOException {
        Element element = doc.createElement("Dataset");

        if (data != null) {
            Element dataElement = doc.createElement("Data");
            byte[] bytes = Utils.readAllBytes(data);
            dataElement.appendChild(doc.createTextNode(Base64.encode(bytes)));
            element.appendChild(dataElement);
        } else if (url != null) {
            Element urlElement = doc.createElement("Uri");
            urlElement.appendChild(doc.createTextNode(url.toString()));
            element.appendChild(urlElement);
        }

        return element;
    }
}
