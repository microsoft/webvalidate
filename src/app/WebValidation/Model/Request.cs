using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    public class Request
    {
        public string Verb { get; set; } = "GET";
        public string Path { get; set; }
        public bool FailOnValidationError { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public string ContentMediaType { get; set; }

        public PerfTarget PerfTarget { get; set; }
        public Validation Validation { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "json serialization")]
    public class InputJson
    {
        public List<string> Variables { get; set; }
        public List<Request> Requests { get; set; }

    }
}
