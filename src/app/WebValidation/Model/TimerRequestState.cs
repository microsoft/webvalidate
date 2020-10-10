using System;
using System.Threading;

namespace CSE.WebValidate
{
    /// <summary>
    /// Shared state for the Timer Request Tasks
    /// </summary>
    internal class TimerRequestState
    {
        public int Index;
        public int MaxIndex;
        public long Count;
        public double Duration;
        public int ErrorCount;
        public Random Random;
        public object Lock = new object();
        public WebV Test;
        public DateTime CurrentLogTime;
        public CancellationToken Token;
    }
}
