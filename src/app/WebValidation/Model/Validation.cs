// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Contains the validation values to use
    /// </summary>
    public class Validation
    {
        /// <summary>
        /// gets or sets the request status code
        /// default: 200
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// gets or sets the request content type
        /// default: application/json
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// gets or sets the content length
        /// default: null (ignore)
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// gets or sets the minimum content length
        /// default: null (ignore)
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// gets or sets the maximum content length
        /// default: null (ignore)
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// gets or sets the maximum ms for the request
        /// default: null (ignore)
        /// </summary>
        public int? MaxMilliseconds { get; set; }

        /// <summary>
        /// gets or sets the list of strings that must be in the response
        /// default: empty list (ignore)
        /// </summary>
        public List<string> Contains { get; set; } = new List<string>();

        /// <summary>
        /// gets or sets the list of strings that can't be in the response
        /// default: empty list (ignore)
        /// </summary>
        public List<string> NotContains { get; set; } = new List<string>();

        /// <summary>
        /// gets or sets the string that must exactly match the response
        /// </summary>
        public string ExactMatch { get; set; }

        /// <summary>
        /// gets or sets the json array properties
        /// </summary>
        public JsonArray JsonArray { get; set; }

        /// <summary>
        /// gets or sets the json object properties
        /// </summary>
        public List<JsonItem> JsonObject { get; set; }

        /// <summary>
        /// gets or sets the performance target
        /// </summary>
        public PerfTarget PerfTarget { get; set; }
    }
}
