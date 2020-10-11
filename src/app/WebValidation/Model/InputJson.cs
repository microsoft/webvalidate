// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// InputJson object
    /// </summary>
    public class InputJson
    {
        /// <summary>
        /// Gets or sets list of variables
        /// </summary>
        public List<string> Variables { get; set; }

        /// <summary>
        /// Gets or sets list of Requests
        /// </summary>
        public List<Request> Requests { get; set; }
    }
}
