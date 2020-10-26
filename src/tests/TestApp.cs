using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CSE.WebValidate.Tests.Unit
{
    public class TestApp
    {
        [Fact]
        public async Task CommandArgsTest()
        {
            // no params displays usage
            Assert.Equal(1, await App.Main(null).ConfigureAwait(false));

            // test remaining valid parameters
            string[] args = new string[] { "--random", "--verbose", "--json-log" };
            Assert.Equal(1, await App.Main(args).ConfigureAwait(false));

            // test bad param
            args = new string[] { "test" };
            Assert.Equal(1, await App.Main(args).ConfigureAwait(false));

            // test bad param with good param
            args = new string[] { "-s", "test", "test" };
            Assert.Equal(1, await App.Main(args).ConfigureAwait(false));
        }

        [Fact]
        public async Task ValidateAllJsonFilesTest()
        {
            // test all files
            Config cfg = new Config
            {
                Server = new List<string> { "http://localhost" },
                Timeout = 30,
                MaxConcurrent = 100
            };

            cfg.Files.Add("msft.json");

            // load and validate all of our test files
            WebV wv = new WebV(cfg);

            // file not found test
            Assert.Null(wv.ReadJson("test"));

            // test with null config
            Assert.NotEqual(0, await wv.RunOnce(null, new System.Threading.CancellationToken()).ConfigureAwait(false));
        }

        [Fact]
        public void EnvironmentVariableTest()
        {
            RootCommand root = App.BuildRootCommand();
            ParseResult parse;

            // set all env vars
            System.Environment.SetEnvironmentVariable(EnvKeys.Files, "msft.json");
            System.Environment.SetEnvironmentVariable(EnvKeys.Server, "test");
            System.Environment.SetEnvironmentVariable(EnvKeys.MaxConcurrent, "100");
            System.Environment.SetEnvironmentVariable(EnvKeys.Random, "false");
            System.Environment.SetEnvironmentVariable(EnvKeys.RequestTimeout, "30");
            System.Environment.SetEnvironmentVariable(EnvKeys.RunLoop, "false");
            System.Environment.SetEnvironmentVariable(EnvKeys.Sleep, "1000");
            System.Environment.SetEnvironmentVariable(EnvKeys.Verbose, "false");
            System.Environment.SetEnvironmentVariable(EnvKeys.VerboseErrors, "false");
            System.Environment.SetEnvironmentVariable(EnvKeys.DelayStart, "1");

            // test env vars
            parse = root.Parse(string.Empty);
            Assert.Equal(0, parse.Errors.Count);
            Assert.Equal(17, parse.CommandResult.Children.Count);

            // override the files env var
            parse = root.Parse("-f file1 file2");
            Assert.Equal(0, parse.Errors.Count);
            Assert.Equal(17, parse.CommandResult.Children.Count);
            Assert.Equal(2, parse.CommandResult.Children.First(c => c.Symbol.Name == "files").Tokens.Count);

            // test run-loop
            System.Environment.SetEnvironmentVariable(EnvKeys.Duration, "30");
            parse = root.Parse(string.Empty);
            Assert.Equal(1, parse.Errors.Count);

            // test run-loop
            System.Environment.SetEnvironmentVariable(EnvKeys.Random, "true");
            parse = root.Parse(string.Empty);
            Assert.Equal(1, parse.Errors.Count);

            // clear env vars
            System.Environment.SetEnvironmentVariable(EnvKeys.Duration, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.Files, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.Server, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.MaxConcurrent, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.Random, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.RequestTimeout, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.RunLoop, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.Sleep, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.Verbose, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.VerboseErrors, null);
            System.Environment.SetEnvironmentVariable(EnvKeys.DelayStart, null);

            // isnullempty fails
            Assert.False(App.CheckFileExists(string.Empty));

            // isnullempty fails
            Assert.False(App.CheckFileExists("testFileNotFound"));
        }

        [Fact]
        public void VersionTest()
        {
            Assert.NotNull(Version.AssemblyVersion);
        }

        [Fact]
        public void FlagTest()
        {
            RootCommand root = App.BuildRootCommand();
            ParseResult parse;

            // bool flags can be specified with just the flag name (-r) or with a value (-v false)
            string args = "-s test -f test.json -r -v false --random true";

            parse = root.Parse(args);

            Assert.Equal(0, parse.Errors.Count);

            SymbolResult result = parse.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop");
            Assert.NotNull(result);
            Assert.Equal(0, result.Tokens.Count);

            result = parse.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "random");
            Assert.NotNull(result);
            Assert.Equal(1, result.Tokens.Count);
            Assert.Equal("true", result.Tokens[0].Value);

            result = parse.CommandResult.Children.FirstOrDefault(c => c.Symbol.Name == "verbose");
            Assert.NotNull(result);
            Assert.Equal(1, result.Tokens.Count);
            Assert.Equal("false", result.Tokens[0].Value);

            args = "-s test -f test.json -r -v false --random badvalue";

            parse = root.Parse(args);

            Assert.Equal(1, parse.Errors.Count);
        }

        private Config BuildConfig(string server)
        {
            App.JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            return new Config
            {
                Server = new List<string> { server },
                Timeout = 10,
                MaxConcurrent = 100,
                MaxErrors = 10,
            };
        }

        [Fact]
        public async Task MsftTest()
        {
            Config cfg = BuildConfig("https://www.microsoft.com");
            cfg.Files.Add("msft.json");

            // load and validate all of our test files
            WebV wv = new WebV(cfg);
            Assert.Equal(0, await wv.RunOnce(cfg, new System.Threading.CancellationToken()).ConfigureAwait(false));
        }

        [Fact]
        public async Task GithubTest()
        {
            Config cfg = BuildConfig("https://api.github.com");
            cfg.Files.Add("github.json");

            // load and validate all of our test files
            WebV wv = new WebV(cfg);
            Assert.Equal(0, await wv.RunOnce(cfg, new System.Threading.CancellationToken()).ConfigureAwait(false));
        }
    }
}
