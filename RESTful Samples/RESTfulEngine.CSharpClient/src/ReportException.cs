/*
 * Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using System;

namespace RESTfulEngine.CSharpClient
{
    [Serializable]
    public class ReportException : Exception
    {
        public ReportException()
            : base()
        {
        }

        public ReportException(string msg)
            : base(msg)
        {
        }
    }
}
