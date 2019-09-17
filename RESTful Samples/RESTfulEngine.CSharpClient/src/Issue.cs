/*
 * Copyright (c) 2017 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// An issue found during the report generation.  Issues are creating
    /// if the error handling and verify functionality is enabled.
    /// The issue represents an error or a warning.
    /// </summary>
    public class Issue
    {
        /// <summary>
        /// A textual description of this issue.
        /// </summary>
        public string Message { get; set; }
    }
}
