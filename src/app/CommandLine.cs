// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;

namespace CSE.WebValidate
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// Build the RootCommand for parsing
        /// </summary>
        /// <returns>RootCommand</returns>
        public static RootCommand BuildRootCommand()
        {
            RootCommand root = new RootCommand
            {
                Name = "WebValidate",
                Description = "Validate API responses",
                TreatUnmatchedTokensAsErrors = true,
            };

            root.AddOption(new Option<string>(new string[] { "-s", "--server" }, ParseString, true, "Server to test"));
            root.AddOption(new Option<List<string>>(new string[] { "-f", "--files" }, ParseStringList, true, "List of files to test"));
            root.AddOption(new Option<string>(new string[] { "--tag" }, ParseString, true, "Tag for log and App Insights"));
            root.AddOption(new Option<int>(new string[] { "-l", "--sleep" }, ParseIntGEZero, true, "Sleep (ms) between each request"));
            root.AddOption(new Option<bool>(new string[] { "-j", "--strict-json" }, ParseBool, true, "Use strict json when parsing"));
            root.AddOption(new Option<string>(new string[] { "-u", "--base-url" }, ParseString, true, "Base url for files"));
            root.AddOption(new Option<bool>(new string[] { "-v", "--verbose" }, ParseBool, true, "Display verbose results"));
            root.AddOption(new Option<bool>(new string[] { "--json-log" }, ParseBool, true, "Use json log format (implies --verbose)"));
            root.AddOption(new Option<bool>(new string[] { "-r", "--run-loop" }, ParseBool, true, "Run test in an infinite loop"));
            root.AddOption(new Option<bool>(new string[] { "--verbose-errors" }, ParseBool, true, "Log verbose error messages"));
            root.AddOption(new Option<bool>(new string[] { "--random" }, ParseBool, true, "Run requests randomly (requires --run-loop)"));
            root.AddOption(new Option<int>(new string[] { "--duration" }, ParseIntGTZero, true, "Test duration (seconds)  (requires --run-loop)"));
            root.AddOption(new Option<int>(new string[] { "--summary-minutes" }, ParseIntGTZero, true, "Display summary results (minutes)  (requires --run-loop)"));
            root.AddOption(new Option<int>(new string[] { "-t", "--timeout" }, ParseIntGEZero, true, "Request timeout (seconds)"));
            root.AddOption(new Option<int>(new string[] { "--max-concurrent" }, ParseIntGTZero, true, "Max concurrent requests"));
            root.AddOption(new Option<int>(new string[] { "--max-errors" }, ParseIntGTZero, true, "Max validation errors"));
            root.AddOption(new Option<int>(new string[] { "--delay-start" }, ParseIntGEZero, true, "Delay test start (seconds)"));
            root.AddOption(new Option<bool>(new string[] { "-d", "--dry-run" }, "Validates configuration"));

            // these require access to --run-loop so are added at the root level
            root.AddValidator(ValidateRunLoopDependencies);

            return root;
        }

        // validate --duration and --random based on --run-loop
        private static string ValidateRunLoopDependencies(CommandResult result)
        {
            OptionResult runLoopRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") as OptionResult;
            OptionResult durationRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "duration") as OptionResult;
            OptionResult randomRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "random") as OptionResult;

            bool runLoop = runLoopRes.GetValueOrDefault<bool>();
            int? duration = durationRes.GetValueOrDefault<int?>();
            bool random = randomRes.GetValueOrDefault<bool>();

            if (duration != null && (int)duration > 0 && !runLoop)
            {
                return "--run-loop must be true to use --duration";
            }

            if (random && !runLoop)
            {
                return "--run-loop must be true to use --random";
            }

            return string.Empty;
        }

        // parse string command line arg
        private static string ParseString(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');
            if (string.IsNullOrEmpty(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return null;
            }

            string val;

            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    if (name == "SERVER")
                    {
                        result.ErrorMessage = $"--{result.Parent.Symbol.Name} is required";
                    }

                    return null;
                }
                else
                {
                    val = env.Trim();
                }
            }
            else
            {
                val = result.Tokens[0].Value.Trim();
            }

            if (string.IsNullOrWhiteSpace(val))
            {
                if (name == "SERVER")
                {
                    result.ErrorMessage = $"--{result.Parent.Symbol.Name} is required";
                }

                return null;
            }
            else if (val.Length < 3)
            {
                result.ErrorMessage = $"--{result.Parent.Symbol.Name} must be at least 3 characters";
                return null;
            }
            else if (val.Length > 100)
            {
                result.ErrorMessage = $"--{result.Parent.Symbol.Name} must be 100 characters or less";
            }

            return val;
        }

        // parse List<string> command line arg (--files)
        private static List<string> ParseStringList(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');
            if (string.IsNullOrEmpty(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return null;
            }

            List<string> val = new List<string>();

            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    result.ErrorMessage = "--files is a required parameter";
                    return null;
                }

                string[] files = env.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (string f in files)
                {
                    val.Add(f.Trim());
                }
            }
            else
            {
                for (int i = 0; i < result.Tokens.Count; i++)
                {
                    val.Add(result.Tokens[i].Value.Trim());
                }
            }

            return val;
        }

        // parse boolean command line arg
        private static bool ParseBool(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrEmpty(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return false;
            }

            string errorMessage = $"--{result.Parent.Symbol.Name} must be true or false";
            bool val;

            // bool options default to true if value not specified (ie -r and -r true)
            if (result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == result.Parent.Symbol.Name) is OptionResult res &&
                !res.IsImplicit &&
                result.Tokens.Count == 0)
            {
                return true;
            }

            // nothing to validate
            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (!string.IsNullOrWhiteSpace(env))
                {
                    if (bool.TryParse(env, out val))
                    {
                        return val;
                    }
                    else
                    {
                        result.ErrorMessage = errorMessage;
                        return false;
                    }
                }

                // default to true
                if (result.Parent.Symbol.Name == "verbose-errors")
                {
                    return true;
                }

                if (result.Parent.Symbol.Name == "verbose" &&
                    result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") is OptionResult resRunLoop &&
                    !resRunLoop.GetValueOrDefault<bool>())
                {
                    return true;
                }

                return false;
            }

            if (!bool.TryParse(result.Tokens[0].Value, out val))
            {
                result.ErrorMessage = errorMessage;
                return false;
            }

            return val;
        }

        // parser for integer >= 0
        private static int ParseIntGEZero(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrEmpty(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return -1;
            }

            int val;
            string errorMessage = $"--{result.Parent.Symbol.Name} must be an integer >= 0";

            // nothing to validate
            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    return GetCommandDefaultValues(result);
                }
                else
                {
                    if (!int.TryParse(env, out val) || val < 0)
                    {
                        result.ErrorMessage = errorMessage;
                        return -1;
                    }

                    return val;
                }
            }

            if (!int.TryParse(result.Tokens[0].Value, out val) || val < 0)
            {
                result.ErrorMessage = errorMessage;
                return -1;
            }

            return val;
        }

        // parser for integer >= 0
        private static int ParseIntGTZero(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrEmpty(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return -1;
            }

            string errorMessage = $"--{result.Parent.Symbol.Name} must be an integer >= 1";
            int val;

            // nothing to validate
            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    return GetCommandDefaultValues(result);
                }
                else
                {
                    if (!int.TryParse(env, out val) || val < 1)
                    {
                        result.ErrorMessage = errorMessage;
                        return -1;
                    }

                    return val;
                }
            }

            if (!int.TryParse(result.Tokens[0].Value, out val) || val < 1)
            {
                result.ErrorMessage = errorMessage;
                return -1;
            }

            return val;
        }

        // get default values for command line args
        private static int GetCommandDefaultValues(ArgumentResult result)
        {
            switch (result.Parent.Symbol.Name)
            {
                case "max-errors":
                    return 10;
                case "max-concurrent":
                    return 100;
                case "sleep":
                    // check run-loop
                    if (result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") is OptionResult res && res.GetValueOrDefault<bool>())
                    {
                        return 1000;
                    }

                    return 0;
                case "timeout":
                    return 30;
                default:
                    return 0;
            }
        }

        // handle --dry-run
        private static int DoDryRun(Config config)
        {
            // display the config
            Console.WriteLine("dry run");
            Console.WriteLine($"   Server          {config.Server}");
            Console.WriteLine($"   Files (count)   {config.Files.Count}");
            if (!string.IsNullOrWhiteSpace(config.Tag))
            {
                Console.WriteLine($"   Tag             {config.Tag}");
            }

            Console.WriteLine($"   Run Loop        {config.RunLoop}");
            Console.WriteLine($"   Sleep           {config.Sleep}");
            Console.WriteLine($"   Verbose Errors  {config.VerboseErrors}");
            Console.WriteLine($"   Strict Json     {config.StrictJson}");
            Console.WriteLine($"   Duration        {config.Duration}");
            Console.WriteLine($"   Delay Start     {config.DelayStart}");
            Console.WriteLine($"   Max Concurrent  {config.MaxConcurrent}");
            Console.WriteLine($"   Max Errors      {config.MaxErrors}");
            Console.WriteLine($"   Random          {config.Random}");
            Console.WriteLine($"   Timeout         {config.Timeout}");
            Console.WriteLine($"   Verbose         {config.Verbose}");

            return 0;
        }
    }
}
