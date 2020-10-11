// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Request Object
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Gets or sets the request verb
        /// default: GET
        /// </summary>
        public string Verb { get; set; } = "GET";

        /// <summary>
        /// Gets or sets the request path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether validation should fail on error
        /// </summary>
        public bool FailOnValidationError { get; set; }

        /// <summary>
        /// Gets or sets the request body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets the request header dictionary
        /// </summary>
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the request media type
        /// </summary>
        public string ContentMediaType { get; set; }

        /// <summary>
        /// Gets or sets the request performance target
        /// </summary>
        public PerfTarget PerfTarget { get; set; }

        /// <summary>
        /// Gets or sets the request validation object
        /// </summary>
        public Validation Validation { get; set; }
    }
}
