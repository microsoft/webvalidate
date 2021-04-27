// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSE.WebValidate.Model;
using CSE.WebValidate.Validators;
using Microsoft.CorrelationVector;

namespace CSE.WebValidate
{
    /// <summary>
    /// Web Validation Test
    /// </summary>
    public partial class WebV
    {
        private static List<Request> requestList;
        private readonly Dictionary<string, PerfTarget> targets = new Dictionary<string, PerfTarget>();
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

                using HttpClient client = OpenClient(ndx);

                // send each request
                foreach (Request r in requestList)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        // stop after MaxErrors errors
                        if ((errorCount + validationFailureCount) >= config.MaxErrors)
                        {
                            break;
                        }

                        // execute the request
                        pl = await ExecuteRequest(client, config.Server[ndx], r).ConfigureAwait(false);

                        if (pl.Failed)
                        {
                            errorCount++;
                        }

                        if (!pl.Failed && !pl.Validated)
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

                // write test summary in xml
                if (config.XmlSummary)
                {
                    // todo - implement standard xml
                    TestSummary res = new TestSummary
                    {
                        ValidationErrorCount = validationFailureCount,
                        ErrorCount = errorCount,
                        MaxErrors = config.MaxErrors,
                    };

                    res.WriteXmlToConsole();
                }
                else if (config.LogFormat == LogFormat.Tsv)
                {
                    // log validation failure count
                    if (validationFailureCount > 0)
                    {
                        Console.WriteLine($"Validation Errors: {validationFailureCount}");
                    }

                    // log error count
                    if (errorCount > 0)
                    {
                        Console.WriteLine($"Failed: {errorCount} Errors");
                    }

                    // log MaxErrors exceeded
                    if (errorCount + validationFailureCount >= config.MaxErrors)
                    {
                        Console.Write($"Failed: Errors: {errorCount + validationFailureCount} >= MaxErrors: {config.MaxErrors}");
                    }
                }
            }

            // return non-zero exit code on failure
            return errorCount > 0 || validationFailureCount >= config.MaxErrors ? errorCount + validationFailureCount : 0;
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

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

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

            List<TimerRequestState> states = new List<TimerRequestState>();

            foreach (string svr in config.Server)
            {
                // create the shared state
                TimerRequestState state = new TimerRequestState
                {
                    Server = svr,
                    Client = OpenHttpClient(svr),
                    MaxIndex = requestList.Count,
                    Test = this,
                    RequestList = requestList,

                    // current hour
                    CurrentLogTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0),

                    Token = token,
                };

                if (config.Random)
                {
                    state.Random = new Random(DateTime.UtcNow.Millisecond);
                }

                states.Add(state);

                // todo - remove in v2.0
                state.Run(config.Sleep, config.MaxConcurrent);
            }

            int frequency = int.MaxValue;
            int initialDelay = int.MaxValue;

            // todo - remove in v2.0
            if (config.SummaryMinutes > 0)
            {
                foreach (TimerRequestState trs in states)
                {
                    // get current summary
                    int cMin = DateTime.UtcNow.Minute / config.SummaryMinutes * config.SummaryMinutes;
                    trs.CurrentLogTime = trs.CurrentLogTime.AddMinutes(cMin);
                    initialDelay = (int)trs.CurrentLogTime.AddMinutes(config.SummaryMinutes).Subtract(DateTime.UtcNow).TotalMilliseconds;
                    frequency = config.SummaryMinutes * 60 * 1000;

                    // start the summary log timer
                    using Timer logTimer = new Timer(new TimerCallback(SummaryLogTask), trs, initialDelay, frequency);
                }
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
        /// <returns>PerfLog</returns>
        public async Task<PerfLog> ExecuteRequest(HttpClient client, string server, Request request)
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

            // send the request
            using (HttpRequestMessage req = new HttpRequestMessage(new HttpMethod(request.Verb), request.Path))
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
                CorrelationVector cv = new CorrelationVector(CorrelationVectorVersion.V2);
                req.Headers.Add(CorrelationVector.HeaderName, cv.Value);

                // add the body to the http request
                if (!string.IsNullOrEmpty(request.Body))
                {
                    if (!string.IsNullOrEmpty(request.ContentMediaType))
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
                    valid = ResponseValidator.Validate(request, resp, body);

                    // check the performance
                    perfLog = CreatePerfLog(server, request, valid, duration, (long)resp.Content.Headers.ContentLength, (int)resp.StatusCode, cv.Value);
                }
                catch (Exception ex)
                {
                    double duration = Math.Round(DateTime.UtcNow.Subtract(dt).TotalMilliseconds, 0);
                    valid = new ValidationResult { Failed = true };
                    valid.ValidationErrors.Add($"Exception: {ex.Message}");
                    perfLog = CreatePerfLog(server, request, valid, duration, 0, 500, cv.Value);
                }
            }

            // log the test
            LogToConsole(request, valid, perfLog);

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
            PerfLog log = new PerfLog(validationResult.ValidationErrors)
            {
                Server = server,
                Tag = string.IsNullOrWhiteSpace(request.Tag) ? config.Tag : request.Tag,
                Path = request?.Path ?? string.Empty,
                StatusCode = statusCode,
                Category = request?.PerfTarget?.Category ?? string.Empty,
                Validated = !validationResult.Failed && validationResult.ValidationErrors.Count == 0,
                Duration = duration,
                ContentLength = contentLength,
                Failed = validationResult.Failed,
                Verb = request.Verb,
                CorrelationVector = correlationVector,
            };

            // determine the Performance Level based on category
            if (targets.ContainsKey(log.Category))
            {
                // lookup the target
                PerfTarget target = targets[log.Category];

                if (target != null &&
                    !string.IsNullOrEmpty(target.Category) &&
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

        // Display the startup message for RunLoop
        private static void DisplayStartupMessage(Config config)
        {
            // don't display if json logging is on
            if (config.LogFormat == LogFormat.Json || config.SummaryMinutes > 0)
            {
                return;
            }

            string msg = $"{Now}\tStarting Web Validation Test";
            msg += $"\n\t\tVersion: {Version.AssemblyVersion}";
            msg += $"\n\t\tHost: {string.Join(' ', config.Server)}";

            if (!string.IsNullOrEmpty(config.Tag))
            {
                msg += $"\n\t\tTag: {config.Tag}";
            }

            if (!string.IsNullOrEmpty(config.BaseUrl))
            {
                msg += $"\n\t\tBaseUrl: {config.BaseUrl}";
            }

            msg += $"\n\t\tFiles: {string.Join(' ', config.Files)}";
            msg += $"\n\t\tSleep: {config.Sleep}";
            msg += $"\n\t\tMaxConcurrent: {config.MaxConcurrent}";

            if (config.Duration > 0)
            {
                msg += $"\n\t\tDuration: {config.Duration}";
            }

            msg += config.Random ? "\n\t\tRandom" : string.Empty;
            msg += config.Verbose ? "\n\t\tVerbose" : string.Empty;

            Console.WriteLine(msg + "\n");
        }

        /// <summary>
        /// Open an http client
        /// </summary>
        /// <param name="index">index of base URL</param>
        private HttpClient OpenClient(int index)
        {
            if (index < 0 || index >= config.Server.Count)
            {
                throw new ArgumentException($"Index out of range: {index}", nameof(index));
            }

            return OpenHttpClient(config.Server[index]);
        }

        /// <summary>
        /// Opens and configures the shared HttpClient
        ///
        /// Disposed in IDispose
        /// </summary>
        /// <returns>HttpClient</returns>
        private HttpClient OpenHttpClient(string host)
        {
            HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                Timeout = new TimeSpan(0, 0, config.Timeout),
                BaseAddress = new Uri(host),
            };
            client.DefaultRequestHeaders.Add("User-Agent", $"webv/{Version.ShortVersion}");

            return client;
        }

        // Summarize the requests for the hour
        private void SummaryLogTask(object timerState)
        {
            // todo - remove in v2.0
            if (config.SummaryMinutes < 1)
            {
                return;
            }

            if (timerState is TimerRequestState state)
            {
                // exit if cancelled
                if (state.Token.IsCancellationRequested)
                {
                    return;
                }

                // build the log entry
                string log = "{ \"logType\": \"summary\", " + $"\"logDate\": \"{state.CurrentLogTime.ToString("o", CultureInfo.InvariantCulture)}Z\", \"tag\": \"{config.Tag}\", ";

                // get the summary values
                lock (state.Lock)
                {
                    log += $"\"requestCount\": {state.Count}, ";
                    log += $"\"averageDuration\": {(state.Count > 0 ? Math.Round(state.Duration / state.Count, 2) : 0)}, ";
                    log += $"\"errorCount\": {state.ErrorCount} " + "}";

                    // reset counters
                    state.Count = 0;
                    state.Duration = 0;
                    state.ErrorCount = 0;

                    // set next log time
                    state.CurrentLogTime = state.CurrentLogTime.AddMinutes(config.SummaryMinutes);
                }

                // log the summary
                Console.WriteLine(log);
            }
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

            switch (config.LogFormat)
            {
                case LogFormat.Json:
                    Console.WriteLine(perfLog.ToJson(config.VerboseErrors));
                    break;
                case LogFormat.Tsv:
                    LogToTsv(request, valid, perfLog);
                    break;
                case LogFormat.None:
                    break;
                default:
                    break;
            }
        }

        // Log the test result to TSV
        private void LogToTsv(Request request, ValidationResult valid, PerfLog perfLog)
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

            if (config.LogFormat == LogFormat.Tsv)
            {
                // check XmlSummary
                if (!config.XmlSummary || (config.XmlSummary && config.Verbose))
                {
                    // only log 4XX and 5XX status codes unless verbose is true or there were validation errors
                    if (config.Verbose || perfLog.StatusCode > 399 || valid.Failed || valid.ValidationErrors.Count > 0)
                    {
                        // log tab delimited
                        string log = $"{perfLog.Date.ToString("o", CultureInfo.InvariantCulture)}\t{perfLog.Server}\t{perfLog.StatusCode}\t{valid.ValidationErrors.Count}\t{perfLog.Duration}\t{perfLog.ContentLength}\t{perfLog.CorrelationVector}\t";

                        // log tag if set
                        if (string.IsNullOrEmpty(perfLog.Tag))
                        {
                            perfLog.Tag = "-";
                        }

                        // default quartile to -
                        string quartile = "-";

                        if (string.IsNullOrEmpty(perfLog.Category))
                        {
                            perfLog.Category = "-";
                        }

                        if (perfLog.Quartile != null && perfLog.Quartile > 0 && perfLog.Quartile <= 4)
                        {
                            quartile = perfLog.Quartile.ToString();
                        }

                        log += $"{perfLog.Tag}\t{quartile}\t{perfLog.Category}\t{request.Verb}\t{perfLog.Path}";

                        // log error details
                        if (config.VerboseErrors && valid.ValidationErrors.Count > 0)
                        {
                            log += "\n  " + string.Join("\n  ", perfLog.Errors);
                        }

                        Console.WriteLine(log);
                    }
                }
            }
        }
    }
}
