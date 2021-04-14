﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace CSE.WebValidate
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        // capture parse errors from env vars
        private static readonly List<string> EnvVarErrors = new List<string>();

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

            root.AddOption(EnvVarOptionList(new string[] { "--server", "-s" }, "Server(s) to test (required)", null));
            root.AddOption(EnvVarOptionList(new string[] { "--files", "-f" }, "List of files to test (required)", null));
            root.AddOption(EnvVarOption(new string[] { "--tag" }, "Tag for log and App Insights", string.Empty));
            root.AddOption(EnvVarOption(new string[] { "--sleep", "-l" }, "Sleep (ms) between each request", 0, 0));
            root.AddOption(EnvVarOption(new string[] { "--strict-json", "-j" }, "Use strict json when parsing", false));
            root.AddOption(EnvVarOption(new string[] { "--base-url", "-u" }, "Base url for files", string.Empty));
            root.AddOption(EnvVarOption(new string[] { "--verbose", "-v" }, "Display verbose results", false));
            root.AddOption(EnvVarOption(new string[] { "--json-log" }, "Use json log format (implies --verbose)", false));
            root.AddOption(EnvVarOption(new string[] { "--run-loop", "-r" }, "Run test in an infinite loop", false));
            root.AddOption(EnvVarOption(new string[] { "--verbose-errors" }, "Log verbose error messages", false));
            root.AddOption(EnvVarOption(new string[] { "--random" }, "Run requests randomly (requires --run-loop)", false));
            root.AddOption(EnvVarOption(new string[] { "--duration" }, "Test duration (seconds)  (requires --run-loop)", 0, 0));
            root.AddOption(EnvVarOption(new string[] { "--summary-minutes" }, "Display summary results (minutes)  (requires --run-loop)", 0, 0));
            root.AddOption(EnvVarOption(new string[] { "--timeout", "-t" }, "Request timeout (seconds)", 30, 1));
            root.AddOption(EnvVarOption(new string[] { "--max-concurrent" }, "Max concurrent requests", 100, 1));
            root.AddOption(EnvVarOption(new string[] { "--max-errors" }, "Max validation errors", 10, 0));
            root.AddOption(EnvVarOption(new string[] { "--delay-start" }, "Delay test start (seconds)", 0, 0));
            root.AddOption(EnvVarOption(new string[] { "--webv-prefix" }, "Server address prefix", "https://"));
            root.AddOption(EnvVarOption(new string[] { "--webv-suffix" }, "Server address suffix", ".azurewebsites.net"));
            root.AddOption(new Option<bool>(new string[] { "--dry-run", "-d" }, "Validates configuration"));

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
            OptionResult serverRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "server") as OptionResult;
            OptionResult filesRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "files") as OptionResult;

            List<string> servers = serverRes.GetValueOrDefault<List<string>>();
            List<string> files = serverRes.GetValueOrDefault<List<string>>();
            bool runLoop = runLoopRes.GetValueOrDefault<bool>();
            int? duration = durationRes.GetValueOrDefault<int?>();
            bool random = randomRes.GetValueOrDefault<bool>();

            string errors = string.Empty;

            if (servers == null || servers.Count == 0)
            {
                errors += "--server must be provided\n";
            }

            if (files == null || files.Count == 0)
            {
                errors += "--files must be provided\n";
            }

            if (duration != null && duration > 0 && !runLoop)
            {
                errors += "--run-loop must be true to use --duration\n";
            }

            if (random && !runLoop)
            {
                errors += "--run-loop must be true to use --random\n";
            }

            return errors;
        }

        // insert env vars as default
        private static Option EnvVarOption<T>(string[] names, string description, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            // this will throw on bad names
            string env = GetValueFromEnvironment(names, out string key);

            T value = defaultValue;

            // set default to environment value if set
            if (!string.IsNullOrWhiteSpace(env))
            {
                if (defaultValue.GetType().IsEnum)
                {
                    if (Enum.TryParse(defaultValue.GetType(), env, true, out object result))
                    {
                        value = (T)result;
                    }
                    else
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }
                }
                else
                {
                    try
                    {
                        value = (T)Convert.ChangeType(env, typeof(T));
                    }
                    catch
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }
                }
            }

            return new Option<T>(names, () => value, description);
        }

        // insert env vars as default
        private static Option<List<string>> EnvVarOptionList(string[] names, string description, List<string> defaultValue)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            // this will throw on bad names
            string env = GetValueFromEnvironment(names, out string key);

            List<string> value = defaultValue;

            // set default to environment value if set
            if (!string.IsNullOrWhiteSpace(env))
            {
                string[] items = env.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                value = new List<string>(items);
            }

            return new Option<List<string>>(names, () => value, description);
        }

        // insert env vars as default with min val for ints
        private static Option<int> EnvVarOption(string[] names, string description, int defaultValue, int minValue)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            // this will throw on bad names
            string env = GetValueFromEnvironment(names, out string key);

            int value = defaultValue;

            // set default to environment value if set
            if (!string.IsNullOrWhiteSpace(env))
            {
                if (!int.TryParse(env, out value))
                {
                    EnvVarErrors.Add($"Environment variable {key} is invalid");
                }
            }

            Option<int> opt = new Option<int>(names, () => value, description);

            opt.AddValidator((res) =>
            {
                string s = string.Empty;
                int val;

                try
                {
                    val = (int)res.GetValueOrDefault();

                    if (val < minValue)
                    {
                        s = $"{names[0]} must be >= {minValue}";
                    }
                }
                catch
                {
                }

                return s;
            });

            return opt;
        }

        // check for environment variable value
        private static string GetValueFromEnvironment(string[] names, out string key)
        {
            if (names == null ||
                names.Length < 1 ||
                names[0].Trim().Length < 4)
            {
                throw new ArgumentNullException(nameof(names));
            }

            for (int i = 1; i < names.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(names[i]) ||
                    names[i].Length != 2 ||
                    names[i][0] != '-')
                {
                    throw new ArgumentException($"Invalid command line parameter at position {i}", nameof(names));
                }
            }

            key = names[0][2..].Trim().ToUpperInvariant().Replace('-', '_');

            return Environment.GetEnvironmentVariable(key);
        }

        // handle --dry-run
        private static int DoDryRun(Config config)
        {
            // display the config
            Console.WriteLine("dry run");
            Console.WriteLine($"   Server          {string.Join(' ', config.Server)}");
            Console.WriteLine($"   Files           {string.Join(' ', config.Files)}");
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

        // Display the ASCII art file if it exists
        private static void DisplayAsciiArt(string[] args, string file)
        {
            if (args != null &&
                !args.Contains("--version") &&
                (args.Contains("-h") ||
                 args.Contains("--help") ||
                 args.Contains("--dry-run") ||
                 args.Contains("-d")))
            {
                string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), file);

                if (File.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine(File.ReadAllText(path));
                    Console.ResetColor();
                }
            }
        }
    }
}
