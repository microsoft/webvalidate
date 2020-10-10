using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace CSE.WebValidate
{
    public sealed partial class App
    {
        // public properties
        public static CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command Line Parameters</param>
        public static async Task<int> Main(string[] args)
        {
            // add ctl-c handler
            AddControlCHandler();

            // build the System.CommandLine.RootCommand
            RootCommand root = BuildRootCommand();
            root.Handler = CommandHandler.Create((Config cfg) => App.Run(cfg));

            if (args == null) args = Array.Empty<string>();

            return await root.InvokeAsync(args).ConfigureAwait(false);
        }

        // System.CommandLine.CommandHandler implementation
        public static async Task<int> Run(Config config)
        {
            if (config == null)
            {
                Console.WriteLine("CommandOptions is null");
                return -1;
            }

            // set any missing values
            config.SetDefaultValues();

            // don't run the test on a dry run
            if (config.DryRun)
            {
                return DoDryRun(config);
            }

            // create the test
            try
            {
                using WebV webv = new CSE.WebValidate.WebV(config);

                if (config.DelayStart > 0)
                {
                    // wait to start the test run
                    Console.WriteLine($"Waiting {config.DelayStart} seconds to start test ...\n");

                    await Task.Delay(config.DelayStart * 1000, TokenSource.Token).ConfigureAwait(false);
                }

                if (config.RunLoop)
                {
                    // run in a loop
                    return webv.RunLoop(config, TokenSource.Token);
                }
                else
                {
                    // run one iteration
                    return await webv.RunOnce(config, TokenSource.Token).ConfigureAwait(false);
                }
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
                Console.WriteLine($"\n{ex}\n\nWebV:Exception:{ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Add a ctl-c handler
        /// </summary>
        private static void AddControlCHandler()
        {
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                const string ControlCMessage = "Ctl-C Pressed - Starting shutdown ...";

                e.Cancel = true;
                TokenSource.Cancel(false);

                Console.WriteLine(ControlCMessage);
            };
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
    }
}
