// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace CSE.WebValidate
{
    /// <summary>
    /// Web Validation Test Configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// gets or sets the server / url
        /// </summary>
        public List<string> Server { get; set; }

        /// <summary>
        /// gets or sets the list of files to read
        /// </summary>
        public List<string> Files { get; set; } = new List<string>();

        /// <summary>
        /// gets or sets the tag to log
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should run in a loop
        /// </summary>
        public bool RunLoop { get; set; }

        /// <summary>
        /// gets or sets the sleep time between requests in ms
        /// </summary>
        public int Sleep { get; set; }

        /// <summary>
        /// gets or sets the duration of the test in seconds
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should use random tests vs. sequential
        /// </summary>
        public bool Random { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logs should be verbose
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logs should be written in json vs. tab delimited
        /// </summary>
        public bool JsonLog { get; set; }

        /// <summary>
        /// gets or sets the the request time out in seconds
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// gets or sets the max concurrent requests
        /// </summary>
        public int MaxConcurrent { get; set; }

        /// <summary>
        /// gets or sets the max errors before the test exits with a non-zero response
        /// </summary>
        public int MaxErrors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should do a dry run
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// gets or sets the base url for test files
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// gets or sets the summary generation time in minutes
        /// </summary>
        public int SummaryMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should display verbose errors or just the count
        /// </summary>
        public bool VerboseErrors { get; set; }

        /// <summary>
        /// gets or sets the seconds to delay before starting the test
        /// </summary>
        public int DelayStart { get; set; }

        /// <summary>
        /// gets or sets a value indicating whether we should use strict json parsing
        /// </summary>
        public bool StrictJson { get; set; }

        /// <summary>
        /// Set the default config values
        /// </summary>
        public void SetDefaultValues()
        {
            if (Server != null && Server.Count > 0)
            {
                string s;

                for (int i = 0; i < Server.Count; i++)
                {
                    s = Server[i];

                    // make it easier to pass server value
                    if (!s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        if (s.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) || s.StartsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                        {
                            Server[i] = $"http://{s}";
                        }
                        else
                        {
                            Server[i] = $"https://{s}.azurewebsites.net";
                        }
                    }
                }
            }

            // add a trailing slash if necessary
            if (!string.IsNullOrEmpty(BaseUrl) && !BaseUrl.EndsWith('/'))
            {
                BaseUrl += "/";
            }
        }
    }
}
