// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// Gets or sets name of test for grouping
        /// </summary>
        public string TestName { get; set; }

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

        /// <summary>
        /// Gets or sets the Region for logging
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the Zone for logging
        /// </summary>
        public string Zone { get; set; }

        /// <summary>
        /// Gets or sets the validation errors
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Gets the json representation of the object
        /// </summary>
        /// <param name="verboseErrors">include verbose errors</param>
        /// <param name="jsonOptions">json serialization options</param>
        /// <returns>json string</returns>
        public string ToJson(bool verboseErrors, JsonSerializerOptions jsonOptions)
        {
            if (verboseErrors)
            {
                return JsonSerializer.Serialize(this, jsonOptions);
            }

            // don't serialize errors
            Errors = null;

            return JsonSerializer.Serialize(this, jsonOptions);
        }

        /// <summary>
        /// Gets the tab separated representation of the object
        /// </summary>
        /// <param name="verboseErrors">include verbose errors</param>
        /// <returns>string</returns>
        public string ToTsv(bool verboseErrors)
        {
            // set missing values
            string quartile = (Quartile != null && Quartile > 0 && Quartile <= 4) ? Quartile.ToString() : "-";
            Tag = string.IsNullOrWhiteSpace(Tag) ? "-" : Tag.Trim();
            Category = string.IsNullOrWhiteSpace(Category) ? "-" : Category.Trim();
            Region = string.IsNullOrWhiteSpace(Region) ? "-" : Region;
            Zone = string.IsNullOrWhiteSpace(Zone) ? "-" : Zone;

            // log tab delimited
            string log = $"{Date:o}\t{TestName}\t{Server}\t{StatusCode}\t{ErrorCount}\t{Duration}\t{ContentLength}\t{Region}\t{Zone}\t{CorrelationVector}\t{Tag}\t{quartile}\t{Category}\t{Verb}\t{Path}";

            // log error details
            if (verboseErrors && ErrorCount > 0)
            {
                log += "\n  " + string.Join("\n  ", Errors);
            }

            return log;
        }

        /// <summary>
        /// Gets the tab separated representation of the object (minimum fields)
        /// </summary>
        /// <returns>string</returns>
        public string ToTsvMin()
        {
            // log tab delimited
            string log = $"{Date:s}\t{StatusCode}\t{ErrorCount}\t{Duration}\t{ContentLength}\t{Verb}\t{Path}";

            return log;
        }
    }
}
