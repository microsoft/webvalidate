// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Performance Log class
    /// </summary>
    public class PerfLog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerfLog"/> class.
        /// </summary>
        /// <param name="validationErrors">list of validation errors</param>
        public PerfLog(List<string> validationErrors)
        {
            Errors = validationErrors;
        }

        /// <summary>
        /// Gets the Type (defaults to request)
        /// </summary>
        public static string Type => "request";

        /// <summary>
        /// Gets or sets the DateTime
        /// </summary>
        public DateTime Date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the http verb
        /// </summary>
        public string Verb { get; set; } = "GET";

        /// <summary>
        /// gets or sets the server URL
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Status Code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the failed flag
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the validated flag
        /// </summary>
        public bool Validated { get; set; } = true;

        /// <summary>
        /// Gets or sets a the correlation vector for distributed tracing
        /// </summary>
        public string CorrelationVector { get; set; }

        /// <summary>
        /// Gets the error count
        /// </summary>
        public int ErrorCount => Errors == null ? 0 : Errors.Count;

        /// <summary>
        /// Gets or sets the duration
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the content length
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// Gets or sets the Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the performance quartile
        /// </summary>
        public int? Quartile { get; set; }

        /// <summary>
        /// Gets or sets the tag
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the request path
        /// </summary>
        public string Path { get; set; }

        public string Region { get; set; }

        public string Zone { get; set; }

        /// <summary>
        /// Gets the validation errors
        /// </summary>
        public List<string> Errors { get; }

        /// <summary>
        /// Gets the json representation of the object
        /// </summary>
        /// <param name="verboseErrors">include verbose errors</param>
        /// <returns>json string</returns>
        public string ToJson(bool verboseErrors)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            if (verboseErrors || Errors == null)
            {
                return JsonSerializer.Serialize(this, options);
            }

            // don't serialize the errors
            string json = JsonSerializer.Serialize(this, options);

            int i = json.IndexOf(",\"errors\":", StringComparison.Ordinal);

            // remove the error messages
            if (i > -1)
            {
                json = json.Substring(0, i) + "}";
            }

            return json;
        }
    }
}
