using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    public class Request
    {
        public string Verb { get; set; } = "GET";
        public string Path { get; set; }
        public bool FailOnValidationError { get; set; } = false;
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public string ContentMediaType { get; set; }

        public PerfTarget PerfTarget { get; set; }
        public Validation Validation { get; set; }
    }
}
