// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

namespace CSE.WebValidate
{
    /// <summary>
    /// Log Format enum
    /// </summary>
    public enum LogFormat
    {
        /// <summary>
        /// Tab Separated Values
        /// </summary>
        Tsv,

        /// <summary>
        /// json
        /// </summary>
        Json,

        /// <summary>
        /// Don't log
        /// --xml-summary and exceptions will still be written to stdout / stderr
        /// </summary>
        None,
    }

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
        /// gets or sets the prefix for server(s)
        /// default: https://
        /// </summary>
        public string WebvPrefix { get; set; }

        /// <summary>
        /// gets or sets the suffix for server(s)
        /// default: .azurewebsites.net
        /// </summary>
        public string WebvSuffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test summary should be written in xml
        /// </summary>
        public bool XmlSummary { get; set; }

        /// <summary>
        /// Gets or sets Log Format
        /// </summary>
        public LogFormat LogFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to expose the :8080/metrics end point for Prometheus
        /// </summary>
        public bool Prometheus { get; set; }

        /// <summary>
        /// Gets or sets the Region deployed to (user defined)
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the Zone deployed to (user defined)
        /// </summary>
        public string Zone { get; set; }

        /// <summary>
        /// Set the default config values
        /// </summary>
        /// <param name="parseResult">system.commandline parse result</param>
        public void SetDefaultValues(ParseResult parseResult = null)
        {
            // default values are different based on --run-loop
            if (parseResult != null)
            {
                if (parseResult.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "verbose") is OptionResult vRes && vRes.IsImplicit)
                {
                    Verbose = !RunLoop && !XmlSummary;
                }

                if (parseResult.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "sleep") is OptionResult sleepRes && sleepRes.IsImplicit)
                {
                    Sleep = RunLoop ? 1000 : 0;
                }

                // todo - remove in v2.0
                if (parseResult.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "json-log") is OptionResult jsonLogRes && !jsonLogRes.IsImplicit)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine("--json-log is deprecated in v2.0 - Use '--log-format json'");
                    Console.ResetColor();
                }

                if (parseResult.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "summary-minutes") is OptionResult summaryRes && !summaryRes.IsImplicit)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine("--summary-minutes is deprecated in v2.0");
                    Console.ResetColor();
                }

                if (parseResult.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "max-concurrent") is OptionResult concurrentRes && !concurrentRes.IsImplicit)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine("--max-concurrent is deprecated in v2.0");
                    Console.ResetColor();
                }

                if (parseResult.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "prometheus") is OptionResult promRes && !promRes.IsImplicit)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine("--prometheus not implemented");
                    Console.ResetColor();
                }
            }

            // todo - remove this in v2.0
            if (JsonLog)
            {
                LogFormat = LogFormat.Json;
            }

            // min sleep is 1ms in --run-loop
            Sleep = RunLoop && Sleep < 1 ? 1 : Sleep;

            // add a trailing slash if necessary
            if (!string.IsNullOrEmpty(BaseUrl) && !BaseUrl.EndsWith('/'))
            {
                BaseUrl += "/";
            }

            if (Server != null && Server.Count > 0)
            {
                string s;

                for (int i = 0; i < Server.Count; i++)
                {
                    s = Server[i];

                    // make it easier to pass server value
                    if (!s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        if (s.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                        {
                            Server[i] = $"http://{s}";
                        }
                        else
                        {
                            Server[i] = $"{WebvPrefix}{s}{WebvSuffix}";
                        }
                    }
                }
            }
        }
    }
}
