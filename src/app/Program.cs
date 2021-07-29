// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CSE.WebValidate
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// Gets or sets json serialization options
        /// </summary>
        public static JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Gets cancellation token
        /// </summary>
        public static CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command Line Parameters</param>
        /// <returns>0 on success</returns>
        public static async Task<int> Main(string[] args)
        {
            // display version info
            // there is a bug in System.CommandLine that causes the built-in version handler to fail
            if (args != null && args.Contains("--version"))
            {
                Console.WriteLine(Version.AssemblyVersion);
                return 0;
            }

            // add ctl-c handler
            AddControlCHandler();

            DisplayAsciiArt(args, "core/ascii-art.txt");

            // build and parse the System.CommandLine.RootCommand
            RootCommand root = BuildRootCommand();
            root.Handler = CommandHandler.Create((Config cfg) => Run(cfg, root.Parse(args)));

            // run the app
            return await root.InvokeAsync(args).ConfigureAwait(false);
        }

        /// <summary>
        /// System.CommandLine.CommandHandler implementation
        /// </summary>
        /// <param name="config">configuration</param>
        /// <param name="parseResult">parse results</param>
        /// <returns>non-zero on failure</returns>
        public static async Task<int> Run(Config config, ParseResult parseResult)
        {
            if (config == null)
            {
                Console.WriteLine("CommandOptions is null");
                return -1;
            }

            // set default values
            config.SetDefaultValues(parseResult);

            // don't run the test on a dry run
            if (config.DryRun)
            {
                return DoDryRun(config);
            }

            // set json options
            if (config.LogFormat == LogFormat.Json || config.LogFormat == LogFormat.JsonCamel)
            {
                // set json options based on --strict-json
                JsonOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    PropertyNameCaseInsensitive = !config.StrictJson,
                    AllowTrailingCommas = !config.StrictJson,
                    ReadCommentHandling = config.StrictJson ? JsonCommentHandling.Disallow : JsonCommentHandling.Skip,
                };

                // set based on json or json pascal
                if (config.LogFormat == LogFormat.JsonCamel)
                {
                    JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    JsonOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                }
            }

            // create the test
            try
            {
                WebV webv = new (config);

                if (config.DelayStart > 0)
                {
                    if (config.LogFormat == LogFormat.Tsv || config.LogFormat == LogFormat.TsvMin)
                    {
                        Console.WriteLine($"Waiting {config.DelayStart} seconds to start test ...\n");
                    }

                    // wait to start the test run
                    await Task.Delay(config.DelayStart * 1000, TokenSource.Token).ConfigureAwait(false);
                }

                int ret;

                if (config.RunLoop)
                {
                    IHost host = null;

                    if (config.Prometheus)
                    {
                        // build and run the web host
                        host = BuildWebHost(config.Port);
                        Task t = host.StartAsync(TokenSource.Token);

                        if (t.IsFaulted)
                        {
                            // stop and dispose the web host
                            await host.StopAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                            host.Dispose();
                            host = null;

                            Console.WriteLine("\n\nUnabled to start web server\n");

                            if (t.Exception != null)
                            {
                                Console.WriteLine(t.Exception.Message);
                            }

                            return -1;
                        }
                    }

                    // run in a loop
                    ret = webv.RunLoop(config, TokenSource.Token);

                    if (host != null)
                    {
                        // stop and dispose the web host
                        await host.StopAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                        host.Dispose();
                        host = null;
                    }

                    // write the stop message
                    if (config.LogFormat == LogFormat.Json || config.LogFormat == LogFormat.JsonCamel)
                    {
                        Console.WriteLine(JsonSerializer.Serialize(
                            new Dictionary<string, object>
                            {
                            { "Date", DateTime.UtcNow },
                            { "EventType", "Shutdown" },
                            },
                            JsonOptions));
                    }
                }
                else
                {
                    // run one iteration
                    ret = await webv.RunOnce(config, TokenSource.Token).ConfigureAwait(false);
                }

                return ret;
            }
            catch (TaskCanceledException tce)
            {
                // log exception
                if (!tce.Task.IsCompleted)
                {
                    Console.WriteLine($"Exception: {tce}");
                    return 1;
                }

                // task is completed
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nException:{ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Check to see if the file exists in the current directory
        /// </summary>
        /// <param name="name">file name</param>
        /// <returns>bool</returns>
        public static bool CheckFileExists(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && System.IO.File.Exists(name.Trim());
        }

        /// <summary>
        /// Add a ctl-c handler
        /// </summary>
        private static void AddControlCHandler()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                TokenSource.Cancel();
            };
        }

        // build the web host
        private static IHost BuildWebHost(int port)
        {
            // configure the web host builder
            return Host.CreateDefaultBuilder()
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                            webBuilder.UseUrls($"http://*:{port}/");
                        })
                        .UseConsoleLifetime()
                        .Build();
        }
    }
}
