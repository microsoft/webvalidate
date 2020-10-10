using System;
using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
    public class PerfLog
    {
        private int errorCount = -1;

        public static string Type { get { return "request"; } }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int StatusCode { get; set; }
        public bool Failed { get; set; }
        public bool Validated { get; set; } = true;
        public int ErrorCount
        {
            set
            {
                errorCount = value;
            }
            get
            {
                if (errorCount > 0)
                {
                    return errorCount;
                }

                return ValidationErrors == null ? 0 : ValidationErrors.Count;
            }
        }
        public double Duration { get; set; }
        public long ContentLength { get; set; }
        public string Category { get; set; }
        public int? Quartile { get; set; }
        public string Tag { get; set; }
        public string Path { get; set; }
        public List<string> ValidationErrors { get; set; }
    }
}
