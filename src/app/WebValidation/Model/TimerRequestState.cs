// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSE.WebValidate.Model;

namespace CSE.WebValidate
{
    /// <summary>
    /// Shared state for the Timer Request Tasks
    /// </summary>
    internal class TimerRequestState
    {
        /// <summary>
        /// gets or sets the server name
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// gets or sets the http client to use
        /// </summary>
        public HttpClient Client { get; set; }

        /// <summary>
        /// gets or sets the request index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// gets or sets the max request index
        /// </summary>
        public int MaxIndex { get; set; }

        /// <summary>
        /// gets or sets the count
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// gets or sets the duration in ms
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// gets or sets the number of errors
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// gets or sets the random number generator
        /// </summary>
        public Random Random { get; set; }

        /// <summary>
        /// gets the lock object
        /// </summary>
        public object Lock { get; } = new object();

        /// <summary>
        /// gets or sets the WebV object
        /// </summary>
        public WebV Test { get; set; }

        /// <summary>
        /// gets or sets the current date time
        /// </summary>
        public DateTime CurrentLogTime { get; set; }

        /// <summary>
        /// gets or sets the cancellation token
        /// </summary>
        public CancellationToken Token { get; set; }

        public List<Request> RequestList { get; set; }

        private string Now { get { return DateTime.UtcNow.ToString("s") + "Z"; } }

        public void Run(double interval)
        {
            timer = new System.Timers.Timer(interval);
            timer.Enabled = true;
            timer.Elapsed += TimerEvent;
            timer.Start();
        }

        private System.Timers.Timer timer;

        private async void TimerEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            int index = 0;

            // verify http client
            if (Client == null)
            {
                Console.WriteLine($"{Now}\tError\tTimerState http client is null");
                return;
            }

            // exit if cancelled
            if (Token.IsCancellationRequested)
            {
                return;
            }

            // lock the state for updates
            lock (Lock)
            {
                index = Index;

                // increment
                Index++;

                // keep the index in range
                if (Index >= MaxIndex)
                {
                    Index = 0;
                }
            }

            Request req = RequestList[index];

            try
            {
                // Execute the request
                PerfLog p = await Test.ExecuteRequest(Client, Server, req).ConfigureAwait(false);

                lock (Lock)
                {
                    // increment
                    Count++;
                    Duration += p.Duration;
                }
            }
            catch (Exception ex)
            {
                // log and ignore any error
                Console.WriteLine($"{Now}\tWebvException\t{ex.Message}");
            }
        }
    }
}
