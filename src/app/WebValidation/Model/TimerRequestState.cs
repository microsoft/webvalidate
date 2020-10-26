// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;

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
    }
}
