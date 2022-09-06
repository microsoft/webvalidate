// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSE.WebValidate.Model;
using CSE.WebValidate.Validators;
using Microsoft.CorrelationVector;
using Prometheus;

namespace CSE.WebValidate
{
    /// <summary>
    /// Web Validation Test
    /// </summary>
    public partial class WebV
    {
        // Prometheus objects
        private static readonly List<string> PrometheusLabels = new () { "status", "server", "failed" };

        // Temporary json file Path
        private static readonly string TempJsonFilePath = "temp/" + Guid.NewGuid() + ".json";

        private static Histogram requestDuration = null;
        private static Summary requestSummary = null;

        private static List<Request> requestList;

        private readonly Dictionary<string, PerfTarget> targets = new ();
        private Config config;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebV"/> class
        /// </summary>
        /// <param name="config">Config</param>
        public WebV(Config config)
        {
            if (config == null || config.Files == null || config.Server == null || config.Server.Count == 0)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.config = config;

            // load the performance targets
            targets = LoadPerfTargets();

            // load the requests from json files
            requestList = LoadValidateRequests(config.Files);

            if (requestList == null || requestList.Count == 0)
            {
                throw new ArgumentException("RequestList is empty");
            }
        }

        /// <summary>
        /// Gets UtcNow as an ISO formatted date string
        /// </summary>
        public static string Now => DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the Prometheus Histogram object
        /// </summary>
        public Histogram RequestDuration
        {
            get
            {
                if (config.Prometheus && requestDuration == null)
                {
                    if (!string.IsNullOrWhiteSpace(config.Region) &&
                        !PrometheusLabels.Contains("region"))
                    {
                        // avoid multi-thread updates
                        lock (PrometheusLabels)
                        {
                            if (!PrometheusLabels.Contains("region"))
                            {
                                PrometheusLabels.Add("region");
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(config.Zone) &&
                        !PrometheusLabels.Contains("zone"))
                    {
                        // avoid multi-thread updates
                        lock (PrometheusLabels)
                        {
                            if (!PrometheusLabels.Contains("zone"))
                            {
                                PrometheusLabels.Add("zone");
                            }
                        }
                    }

                    requestDuration = Metrics.CreateHistogram(
                    "WebVDuration",
                    "Histogram of WebV request duration",
                    new HistogramConfiguration
                    {
                        Buckets = Histogram.ExponentialBuckets(1, 2, 10),
                        LabelNames = PrometheusLabels.ToArray(),
                    });
                }

                return requestDuration;
            }
        }

        /// <summary>
        /// Gets the Prometheus Summary object
        /// </summary>
        public Summary RequestSummary
        {
            get
            {
                if (config.Prometheus && requestSummary == null)
                {
                    if (!string.IsNullOrWhiteSpace(config.Region) &&
                        !PrometheusLabels.Contains("region"))
                    {
                        // avoid multi-thread updates
                        lock (PrometheusLabels)
                        {
                            if (!PrometheusLabels.Contains("region"))
                            {
                                PrometheusLabels.Add("region");
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(config.Zone) &&
                        !PrometheusLabels.Contains("zone"))
                    {
                        // avoid multi-thread updates
                        lock (PrometheusLabels)
                        {
                            if (!PrometheusLabels.Contains("zone"))
                            {
                                PrometheusLabels.Add("zone");
                            }
                        }
                    }

                    requestSummary = Metrics.CreateSummary(
                        "WebVSummary",
                        "Summary of WebV request duration",
                        new SummaryConfiguration
                        {
                            SuppressInitialValue = true,
                            MaxAge = TimeSpan.FromMinutes(5),
                            Objectives = new List<QuantileEpsilonPair> { new QuantileEpsilonPair(.9, .0), new QuantileEpsilonPair(.95, .0), new QuantileEpsilonPair(.99, .0), new QuantileEpsilonPair(1.0, .0) },
                            LabelNames = PrometheusLabels.ToArray(),
                        });
                }

                return requestSummary;
            }
        }

        /// <summary>
        /// Run the validation test one time
        /// </summary>
        /// <param name="config">configuration</param>
        /// <param name="token">cancellation token</param>
        /// <returns>bool</returns>
        public async Task<int> RunOnce(Config config, CancellationToken token)
        {
            if (config == null)
            {
                Console.WriteLine("RunOnce:Config is null");
                return -1;
            }

            int duration;
            PerfLog pl;
            int errorCount = 0;
            int validationFailureCount = 0;

            // check if the temp directory exists for the XML summary case
            if (config.Summary == SummaryFormat.Xml)
            {
                if (!Directory.Exists("temp"))
                {
                    Directory.CreateDirectory("temp");
                }
            }

            // loop through each server
            for (int ndx = 0; ndx < config.Server.Count; ndx++)
            {
                // reset error counts
                if (config.Server.Count > 0)
                {
                    if (ndx > 0)
                    {
                        Console.WriteLine();
                        errorCount = 0;
                        validationFailureCount = 0;
                    }
                }

                using HttpClient client = OpenHttpClient(config.Server[ndx]);

                // Start a stopwatch to calculate total time elapsed for a run
                Stopwatch stopWatch = new ();
                stopWatch.Start();

                // send each request
                foreach (Request r in requestList)
                {
                    // stop on signal or after MaxErrors
                    if (token.IsCancellationRequested || (errorCount + validationFailureCount) >= config.MaxErrors)
                    {
                        break;
                    }

                    try
                    {
                        // execute the request
                        pl = await ExecuteRequest(client, config.Server[ndx], r, config.UrlPrefix).ConfigureAwait(false);

                        if (pl.Failed)
                        {
                            errorCount++;
                        }
                        else if (!pl.Validated)
                        {
                            validationFailureCount++;
                        }

                        // sleep if configured
                        if (config.Sleep > 0)
                        {
                            duration = config.Sleep - (int)pl.Duration;

                            if (duration > 0)
                            {
                                await Task.Delay(duration, token).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore any exception caused by ctl-c or stop signal
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        // log error and keep processing
                        Console.WriteLine($"{Now}\tException: {ex.Message}");
                        errorCount++;
                    }
                }

                stopWatch.Stop();

                DisplaySummary(validationFailureCount, errorCount, stopWatch.Elapsed.TotalSeconds);
            }

            // return non-zero exit code on failure
            return (errorCount > 0 || validationFailureCount >= config.MaxErrors) ? errorCount + validationFailureCount : 0;
        }

        /// <summary>
        /// Run the validation tests in a loop
        /// </summary>
        /// <param name="config">Config</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>0 on success</returns>
        public int RunLoop(Config config, CancellationToken token)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            DateTime dtMax = DateTime.MaxValue;

            // only run for duration (seconds)
            if (config.Duration > 0)
            {
                dtMax = DateTime.UtcNow.AddSeconds(config.Duration);
            }

            if (config.Sleep < 1)
            {
                config.Sleep = 1;
            }

            DisplayStartupMessage(config);

            List<TimerRequestState> states = new ();

            foreach (string svr in config.Server)
            {
                // create the shared state
                TimerRequestState state = new ()
                {
                    Server = svr,
                    Client = OpenHttpClient(svr),
                    MaxIndex = requestList.Count,
                    Test = this,
                    RequestList = requestList,
                    Token = token,
                    UrlPrefix = config.UrlPrefix,
                };

                if (config.Random)
                {
                    state.Random = new Random(DateTime.UtcNow.Millisecond);
                }

                states.Add(state);

                // run the timer proc
                state.Run(config.Sleep);
            }

            try
            {
                // run the wait loop
                if (dtMax == DateTime.MaxValue)
                {
                    Task.Delay(-1, token).Wait(token);
                }
                else
                {
                    // wait one hour to keep total milliseconds from overflowing
                    while (dtMax.Subtract(DateTime.UtcNow).TotalHours > 1)
                    {
                        Task.Delay(60 * 60 * 1000, token).Wait(token);
                    }

                    int delay = (int)dtMax.Subtract(DateTime.UtcNow).TotalMilliseconds;

                    if (delay > 0)
                    {
                        Task.Delay(delay, token).Wait(token);
                    }
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
            catch (OperationCanceledException oce)
            {
                // log exception
                if (!token.IsCancellationRequested)
                {
                    Console.Write($"Exception: {oce}");
                    return 1;
                }

                // Operation was cancelled
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                return -1;
            }

            // graceful exit
            return 0;
        }

        /// <summary>
        /// Execute a single validation test
        /// </summary>
        /// <param name="client">http client</param>
        /// <param name="server">server URL</param>
        /// <param name="request">Request</param>
        /// <param name="urlPrefix">URL prefix</param>
        /// <returns>PerfLog</returns>
        public async Task<PerfLog> ExecuteRequest(HttpClient client, string server, Request request, string urlPrefix)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            PerfLog perfLog;
            ValidationResult valid;

            if (requestDuration == null)
            {
                requestDuration = RequestDuration;
            }

            if (requestSummary == null)
            {
                requestSummary = RequestSummary;
            }

            string path = request.Path;

            if (!string.IsNullOrWhiteSpace(urlPrefix))
            {
                path = urlPrefix + path;
            }

            // send the request
            using (HttpRequestMessage req = new(new HttpMethod(request.Verb), path))
            {
                DateTime dt = DateTime.UtcNow;

                // add the headers to the http request
                if (request.Headers != null && request.Headers.Count > 0)
                {
                    foreach (string key in request.Headers.Keys)
                    {
                        req.Headers.Add(key, request.Headers[key]);
                    }
                }

                // create correlation vector and add to headers
                CorrelationVector cv = new (CorrelationVectorVersion.V2);
                req.Headers.Add(CorrelationVector.HeaderName, cv.Value);

                // add the body to the http request
                if (!string.IsNullOrWhiteSpace(request.Body))
                {
                    if (!string.IsNullOrWhiteSpace(request.ContentMediaType))
                    {
                        req.Content = new StringContent(request.Body, Encoding.UTF8, request.ContentMediaType);
                    }
                    else
                    {
                        req.Content = new StringContent(request.Body);
                    }
                }

                try
                {
                    // process the response
                    using HttpResponseMessage resp = await client.SendAsync(req).ConfigureAwait(false);
                    string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                    double duration = Math.Round(DateTime.UtcNow.Subtract(dt).TotalMilliseconds, 0);

                    // validate the response
                    valid = ResponseValidator.Validate(request, resp, body, duration);

                    // check the performance
                    perfLog = CreatePerfLog(server, request, valid, duration, (long)resp.Content.Headers.ContentLength, (int)resp.StatusCode, cv.Value);

                    if (config.Summary == SummaryFormat.Xml)
                    {
                        // append perflog to file
                        using StreamWriter sw = File.AppendText(TempJsonFilePath);
                        sw.WriteLine(JsonSerializer.Serialize(perfLog));
                    }
                }
                catch (Exception ex)
                {
                    double duration = Math.Round(DateTime.UtcNow.Subtract(dt).TotalMilliseconds, 0);
                    valid = new ValidationResult { Failed = true };
                    valid.ValidationErrors.Add($"Exception: {ex.Message}");
                    perfLog = CreatePerfLog(server, request, valid, duration, 0, 500, cv.Value);

                    if (config.Summary == SummaryFormat.Xml)
                    {
                        // append to file
                        using StreamWriter sw = File.AppendText(TempJsonFilePath);
                        sw.WriteLine(JsonSerializer.Serialize(perfLog));
                    }
                }
            }

            // log the test
            LogToConsole(request, valid, perfLog);

            if (config.Prometheus)
            {
                // map status code to reduce histogram size
                string status = GetPrometheusCode(perfLog.StatusCode);

                List<string> labels = new ()
                {
                    status,
                    perfLog.Server,
                    perfLog.Failed.ToString(),
                };

                if (!string.IsNullOrWhiteSpace(config.Region))
                {
                    labels.Add(config.Region);
                }

                if (!string.IsNullOrWhiteSpace(config.Zone))
                {
                    labels.Add(config.Zone);
                }

                RequestDuration.WithLabels(labels.ToArray()).Observe(perfLog.Duration);
                RequestSummary.WithLabels(labels.ToArray()).Observe(perfLog.Duration);
            }

            return perfLog;
        }

        /// <summary>
        /// Create a PerfLog
        /// </summary>
        /// <param name="server">server URL</param>
        /// <param name="request">Request</param>
        /// <param name="validationResult">validation errors</param>
        /// <param name="duration">duration</param>
        /// <param name="contentLength">content length</param>
        /// <param name="statusCode">status code</param>
        /// <param name="correlationVector">Correlation Vector</param>
        /// <returns>PerfLog</returns>
        public PerfLog CreatePerfLog(string server, Request request, ValidationResult validationResult, double duration, long contentLength, int statusCode, string correlationVector = "")
        {
            if (validationResult == null)
            {
                throw new ArgumentNullException(nameof(validationResult));
            }

            // map the parameters
            PerfLog log = new (validationResult.ValidationErrors)
            {
                Server = server,
                Tag = string.IsNullOrWhiteSpace(request.Tag) ? config.Tag : request.Tag,
                Path = request?.Path ?? string.Empty,
                StatusCode = statusCode,
                Category = request?.PerfTarget?.Category ?? string.Empty,
                TestName = request.TestName,
                Validated = !validationResult.Failed && validationResult.ValidationErrors.Count == 0,
                Duration = duration,
                ContentLength = contentLength,
                Failed = validationResult.Failed,
                Verb = request.Verb,
                CorrelationVector = correlationVector,
                Region = config.Region,
                Zone = config.Zone,
            };

            // determine the Performance Level based on category
            if (targets.ContainsKey(log.Category))
            {
                // lookup the target
                PerfTarget target = targets[log.Category];

                if (target != null &&
                    !string.IsNullOrWhiteSpace(target.Category) &&
                    target.Quartiles != null &&
                    target.Quartiles.Count == 3)
                {
                    // set to max
                    log.Quartile = target.Quartiles.Count + 1;

                    for (int i = 0; i < target.Quartiles.Count; i++)
                    {
                        // find the lowest Perf Target achieved
                        if (duration <= target.Quartiles[i])
                        {
                            log.Quartile = i + 1;
                            break;
                        }
                    }
                }
            }

            return log;
        }

        // convert http status code
        private static string GetPrometheusCode(int statusCode)
        {
            if (statusCode >= 500)
            {
                return "Error";
            }
            else if (statusCode == 429)
            {
                return "Retry";
            }
            else if (statusCode >= 400)
            {
                return "Warn";
            }
            else if (statusCode >= 300)
            {
                return "Redirect";
            }
            else
            {
                return "OK";
            }
        }

        // Display the startup message for RunLoop
        private static void DisplayStartupMessage(Config config)
        {
            // don't display if json logging is on
            if (config.LogFormat == LogFormat.Json || config.LogFormat == LogFormat.JsonCamel)
            {
                Dictionary<string, object> startupDict = new ()
                {
                    { "Date", DateTime.UtcNow },
                    { "EventType", "Startup" },
                    { "Version", Version.AssemblyVersion },
                    { "Host", string.Join(' ', config.Server) },
                    { "Files", string.Join(' ', config.Files) },
                    { "Sleep", config.Sleep },
                    { "Duration", config.Duration },
                    { "Random", config.Random },
                    { "Verbose", config.Verbose },
                };

                if (!string.IsNullOrWhiteSpace(config.BaseUrl))
                {
                    startupDict.Add("Tag", config.BaseUrl);
                }

                if (!string.IsNullOrWhiteSpace(config.Tag))
                {
                    startupDict.Add("Tag", config.Tag);
                }

                if (!string.IsNullOrWhiteSpace(config.Region))
                {
                    startupDict.Add("Region", config.Region);
                }

                if (!string.IsNullOrWhiteSpace(config.Zone))
                {
                    startupDict.Add("Zone", config.Zone);
                }

                Console.WriteLine(JsonSerializer.Serialize(startupDict, App.JsonOptions));

                return;
            }

            string msg = $"{Now}\tStarting Web Validation Test";
            msg += $"\n\t\tVersion: {Version.AssemblyVersion}";
            msg += $"\n\t\tHost: {string.Join(' ', config.Server)}";
            msg += $"\n\t\tURL Prefix: {config.UrlPrefix}";

            if (!string.IsNullOrWhiteSpace(config.Tag))
            {
                msg += $"\n\t\tTag: {config.Tag}";
            }

            if (!string.IsNullOrWhiteSpace(config.BaseUrl))
            {
                msg += $"\n\t\tBaseUrl: {config.BaseUrl}";
            }

            msg += $"\n\t\tFiles: {string.Join(' ', config.Files)}";
            msg += $"\n\t\tSleep: {config.Sleep}";

            if (config.Duration > 0)
            {
                msg += $"\n\t\tDuration: {config.Duration}";
            }

            msg += config.Random ? "\n\t\tRandom" : string.Empty;
            msg += config.Verbose ? "\n\t\tVerbose" : string.Empty;

            Console.WriteLine(msg + "\n");
        }

        // display summary results
        private void DisplaySummary(int validationFailureCount, int errorCount, double totalTestRunDuration)
        {
            string status = (errorCount + validationFailureCount >= config.MaxErrors) ? "Test Failed" : "Test Completed";

            switch (config.Summary)
            {
                case SummaryFormat.Tsv:
                    Console.WriteLine($"{status}\tErrors\t{errorCount}\tValidationErrorCount\t{validationFailureCount}\tMaxErrors\t{config.MaxErrors}");
                    break;

                case SummaryFormat.Json:
                case SummaryFormat.JsonCamel:
                    Dictionary<string, object> summary = new ()
                    {
                        { "Date", DateTime.Now },
                        { "Status", status },
                        { "ValidationErrorCount", validationFailureCount },
                        { "ErrorCount", errorCount },
                        { "MaxErrors", config.MaxErrors },
                    };

                    Console.WriteLine(JsonSerializer.Serialize(summary, App.JsonOptions));
                    break;

                case SummaryFormat.Xml:
                    // Get all the perf logs from temp folder's guid.json, build the summary format xml and output it to console
                    TestSuite testSuite = new ()
                    {
                        Failures = errorCount.ToString(),
                        Name = "WebVToJUnit",
                        Skipped = "0",
                        Tests = requestList.Count.ToString(),
                        Time = totalTestRunDuration.ToString(),
                        TestCases = new List<TestCase>(),
                    };

                    if (File.Exists(TempJsonFilePath))
                    {
                        using StreamReader sr = new (TempJsonFilePath);
                        string ln;

                        while ((ln = sr.ReadLine()) != null)
                        {
                            PerfLog perf = JsonSerializer.Deserialize<PerfLog>(ln);

                            TestCase testCase = new ()
                            {
                                Name = ((perf.Tag == null) ? string.Empty : (perf.Tag + ": ")) + perf.Verb + ": " + perf.Path,
                                Time = TimeSpan.FromMilliseconds(perf.Duration).TotalSeconds.ToString(),
                            };

                            testCase.ClassName = testCase.Name;

                            if (perf.ErrorCount >= 1)
                            {
                                testCase.Failure = new()
                                {
                                    Message = string.Join("\n", perf.Errors),
                                };
                            }
                            else
                            {
                                testCase.SystemOut = string.Empty;
                            }

                            testSuite.TestCases.Add(testCase);
                        }
                    }

                    Console.WriteLine(testSuite.ToXml());

                    // Delete the temp.json file
                    if (File.Exists(TempJsonFilePath))
                    {
                        File.Delete(TempJsonFilePath);
                    }

                    break;

                case SummaryFormat.None:
                default:
                    break;
            }
        }

        // Opens and configures an HttpClient
        private HttpClient OpenHttpClient(string host)
        {
            HttpClient client = new (new HttpClientHandler { AllowAutoRedirect = false })
            {
                Timeout = new TimeSpan(0, 0, config.Timeout),
                BaseAddress = new Uri(host),
            };

            client.DefaultRequestHeaders.Add("User-Agent", $"webv/{Version.ShortVersion}");

            return client;
        }

        // Log the test
        private void LogToConsole(Request request, ValidationResult valid, PerfLog perfLog)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (valid == null)
            {
                throw new ArgumentNullException(nameof(valid));
            }

            if (perfLog == null)
            {
                throw new ArgumentNullException(nameof(perfLog));
            }

            // always log on error
            if (config.Verbose || perfLog.StatusCode >= 400 || perfLog.Failed || perfLog.ErrorCount > 0)
            {
                switch (config.LogFormat)
                {
                    case LogFormat.Json:
                    case LogFormat.JsonCamel:
                        Console.WriteLine(perfLog.ToJson(config.VerboseErrors, App.JsonOptions));
                        break;
                    case LogFormat.Tsv:
                        Console.WriteLine(perfLog.ToTsv(config.VerboseErrors));
                        break;
                    case LogFormat.TsvMin:
                        Console.WriteLine(perfLog.ToTsvMin(config.VerboseErrors));
                        break;
                    case LogFormat.None:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
