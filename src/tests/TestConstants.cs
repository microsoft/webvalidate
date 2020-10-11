using System.Collections.Generic;

namespace CSE.WebValidate.Tests
{
    /// <summary>
    /// Environment Variable Keys
    /// </summary>
    public sealed class EnvKeys
    {
        public const string RunLoop = "RUN_LOOP";
        public const string MaxConcurrent = "MAX_CONCURRENT";
        public const string MaxErrors = "MAX_ERRORS";
        public const string Sleep = "SLEEP";
        public const string Verbose = "VERBOSE";
        public const string Files = "FILES";
        public const string Random = "RANDOM";
        public const string Server = "SERVER";
        public const string Duration = "DURATION";
        public const string RequestTimeout = "TIMEOUT";
        public const string VerboseErrors = "VERBOSE_ERRORS";
        public const string DelayStart = "DELAY_START";

        public static Dictionary<string, string> EnvVarToCommandLineDictionary()
        {
            return new Dictionary<string, string>
            {
                { Server, "--server -s" },
                { Sleep, "--sleep -l" },
                { Verbose, "--verbose -v" },
                { RunLoop, "--run-loop -r" },
                { Random, "--random" },
                { Duration, "--duration" },
                { RequestTimeout, "--timeout -t" },
                { MaxConcurrent, "--max-concurrent" },
                { MaxErrors, "--max-errors" },
                { VerboseErrors, "--verbose-errors" },
                { DelayStart, "--delay-start" }
            };
        }
    }

    /// <summary>
    /// Command Line parameter keys
    /// </summary>
    public sealed class ArgKeys
    {
        public const string RunLoop = "--runloop";
        public const string MaxConcurrent = "--maxconcurrent";
        public const string Sleep = "--sleep";
        public const string Verbose = "--verbose";
        public const string Files = "--files";
        public const string Random = "--random";
        public const string Host = "--host";
        public const string Duration = "--duration";
        public const string RequestTimeout = "--timeout";
        public const string JsonLog = "--json-log";
        public const string SummaryMinutes = "--summary-minutes";
        public const string VerboseErrors = "--verbose-errors";
        public const string DelayStart = "--delay-start";
        public const string Help = "--help";
        public const string HelpShort = "-h";
    }
}
