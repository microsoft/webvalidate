using System;

namespace CSE.WebValidate.Model
{
    public class PerfLog
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Category { get; set; }
        public int PerfLevel { get; set; }
        public bool Validated { get; set; } = true;
        public bool Failed { get; set; }
        public string ValidationResults { get; set; } = string.Empty;
        public double Duration { get; set; }
        public int StatusCode { get; set; }
        public long ContentLength { get; set; }
    }
}
