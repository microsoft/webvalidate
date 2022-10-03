// Copyright (c) Microsoft Corporation. All rights reserved.
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
        private static readonly List<string> EnvVarErrors = new ();

        /// <summary>
        /// Build the RootCommand for parsing
        /// </summary>
        /// <returns>RootCommand</returns>
        public static RootCommand BuildRootCommand()
        {
            RootCommand root = new ()
            {
                Name = "WebValidate",
                Description = "Validate API responses",
                TreatUnmatchedTokensAsErrors = true,
            };

            root.AddOption(EnvVarOption<List<string>>(new string[] { "--files", "-f" }, "List of files to test (required)", null));
            root.AddOption(EnvVarOption<List<string>>(new string[] { "--server", "-s" }, "Server(s) to test (required)", null));
            root.AddOption(EnvVarOption<int>(new string[] { "--port", "-p" }, "Port for web listener  (requires --run-loop)", 8080));
            root.AddOption(EnvVarOption<int>(new string[] { "--delay-start" }, "Delay test start (seconds)", 0, 0));
            root.AddOption(EnvVarOption<int>(new string[] { "--duration" }, "Test duration (seconds)  (requires --run-loop)", 0, 0));
            root.AddOption(EnvVarOption(new string[] { "--log-format", "-g" }, "Log format", LogFormat.TsvMin));
            root.AddOption(EnvVarOption<int>(new string[] { "--max-errors" }, "Max validation errors", 10, 0));
            root.AddOption(EnvVarOption(new string[] { "--random" }, "Run requests randomly (requires --run-loop)", false));
            root.AddOption(EnvVarOption(new string[] { "--region" }, "Region deployed to (user defined)", string.Empty));
            root.AddOption(EnvVarOption(new string[] { "--run-loop", "-r" }, "Run test in an infinite loop", false));
            root.AddOption(EnvVarOption<int>(new string[] { "--sleep", "-l" }, "Sleep (ms) between each request", 0, 0));
            root.AddOption(EnvVarOption(new string[] { "--tag" }, "Tag for log (user defined)", string.Empty));
            root.AddOption(EnvVarOption<int>(new string[] { "--timeout", "-t" }, "Request timeout (seconds)", 30, 1));
            root.AddOption(EnvVarOption(new string[] { "--verbose", "-v" }, "Display all request results", false));
            root.AddOption(EnvVarOption(new string[] { "--verbose-errors" }, "Log verbose error messages", false));
            root.AddOption(EnvVarOption(new string[] { "--summary" }, "Display test summary (invalid with --run-loop)", SummaryFormat.None));
            root.AddOption(EnvVarOption(new string[] { "--zone" }, "Zone deployed to (user defined)", string.Empty));
            root.AddOption(EnvVarOption(new string[] { "--url-prefix", "-u" }, "Url prefix for requests", string.Empty));

            root.AddOption(new Option<bool>(new string[] { "--dry-run", "-d" }, "Validates configuration"));
            root.AddOption(new Option<bool>(new string[] { "--version" }, "Displays version and exits"));

            // these require access to --run-loop so are added at the root level
            root.AddValidator(ValidateRunLoopDependencies);

            return root;
        }

        // validate based on --run-loop
        private static string ValidateRunLoopDependencies(CommandResult result)
        {
            string errors = string.Empty;

            OptionResult serverRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "server") as OptionResult;
            OptionResult filesRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "files") as OptionResult;
            OptionResult durationRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "duration") as OptionResult;
            OptionResult formatRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "log-format") as OptionResult;
            OptionResult portRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "port") as OptionResult;

            bool runLoop = result.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") is OptionResult runLoopRes && runLoopRes.GetValueOrDefault<bool>();
            bool random = result.Children.FirstOrDefault(c => c.Symbol.Name == "random") is OptionResult randomRes && randomRes.GetValueOrDefault<bool>();
            bool verbose = result.Children.FirstOrDefault(c => c.Symbol.Name == "verbose") is OptionResult verboseRes && verboseRes.GetValueOrDefault<bool>();
            bool xml = result.Children.FirstOrDefault(c => c.Symbol.Name == "xml-summary") is OptionResult xmlRes && xmlRes.GetValueOrDefault<bool>();

            List<string> servers = serverRes.GetValueOrDefault<List<string>>();
            List<string> files = serverRes.GetValueOrDefault<List<string>>();

            int duration = 0;
            int port = 8080;
            LogFormat logFormat = LogFormat.TsvMin;

            try
            {
                duration = durationRes.GetValueOrDefault<int>();
                port = portRes.GetValueOrDefault<int>();
                logFormat = formatRes.GetValueOrDefault<LogFormat>();
            }
            catch
            {
                // let system.commandline.parser handle the error
            }

            if (servers == null || servers.Count == 0)
            {
                errors += "--server must be provided\n";
            }

            if (files == null || files.Count == 0)
            {
                errors += "--files must be provided\n";
            }

            if (portRes != null && !portRes.IsImplicit)
            {
                if (!runLoop)
                {
                    errors += "--run-loop must be true to use --port\n";
                }
                else
                {
                    if (port < 1 || port >= 64 * 1024)
                    {
                        errors += $"--port must be > 0 and < {64 * 1024}";
                    }
                }
            }

            if (duration > 0 && !runLoop)
            {
                errors += "--run-loop must be true to use --duration\n";
            }

            if (random && !runLoop)
            {
                errors += "--run-loop must be true to use --random\n";
            }

            if (xml && runLoop)
            {
                errors += "--xml-summary conflicts with --run-loop\n";
            }

            if (verbose && logFormat == LogFormat.None)
            {
                errors += "--verbose conflicts with --log-format None\n";
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
        private static Option EnvVarOption<T>(string[] names, string description, List<string> defaultValue)
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
        private static Option EnvVarOption<T>(string[] names, string description, int defaultValue, int minValue)
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

            Option<int> opt = new (names, () => value, description);

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

            if (!string.IsNullOrWhiteSpace(config.UrlPrefix))
            {
                Console.WriteLine($"   URL Prefix      {config.UrlPrefix}");
            }

            if (config.DelayStart > 0)
            {
                Console.WriteLine($"   Delay Start     {config.DelayStart}");
            }

            if (config.Duration > 0)
            {
                Console.WriteLine($"   Duration        {config.Duration}");
            }

            Console.WriteLine($"   Log Format      {config.LogFormat}");
            Console.WriteLine($"   Log Summary     {config.Summary}");

            if (!config.RunLoop)
            {
                Console.WriteLine($"   Max Errors      {config.MaxErrors}");
            }

            if (config.RunLoop)
            {
                Console.WriteLine($"   Port            {config.Port}");
                Console.WriteLine($"   Random          {config.Random}");
            }

            if (!string.IsNullOrEmpty(config.Region))
            {
                Console.WriteLine($"   Region          {config.Region}");
            }

            Console.WriteLine($"   Run Loop        {config.RunLoop}");
            Console.WriteLine($"   Sleep           {config.Sleep}");

            if (!string.IsNullOrWhiteSpace(config.Tag))
            {
                Console.WriteLine($"   Tag             {config.Tag}");
            }

            Console.WriteLine($"   Timeout         {config.Timeout}");
            Console.WriteLine($"   Verbose         {config.Verbose}");
            Console.WriteLine($"   Verbose Errors  {config.VerboseErrors}");

            if (!string.IsNullOrEmpty(config.Zone))
            {
                Console.WriteLine($"   Zone            {config.Zone}");
            }

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
