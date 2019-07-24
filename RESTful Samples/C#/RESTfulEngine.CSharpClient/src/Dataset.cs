/*
* Copyright (c) 2016 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/

using System;
using System.IO;
using System.Xml.Linq;

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// Defines a dataset. This is a POD file that you want to pass to the engine for processing.
    /// </summary>
    public class Dataset
    {
        private Stream data;
        private Uri uri;

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        /// <param name="data">The input stream to read the POD file from.</param>
        public Dataset(Stream data)
        {
            this.data = data;
        }

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        /// <param name="uri">The location of the POD file. This location must be accessible from the server.</param>
        public Dataset(Uri uri)
        {
            this.uri = uri;
        }

        internal XElement GetXml()
        {
            var element = new XElement("Dataset");

            if (data != null)
            {
                var bytes = Utils.ReadAllBytes(data);
                element.Add(new XElement("Data", Convert.ToBase64String(bytes)));
            }
            else if (uri != null)
            {
                element.Add(new XElement("Uri", uri));
            }

            return element;
        }
    }
}
