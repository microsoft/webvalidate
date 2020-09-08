using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    public class Validation
    {
        public int StatusCode { get; set; } = 200;
        public string ContentType { get; set; } = "application/json";
        public int? Length { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public int? MaxMilliseconds { get; set; }
        public List<string> Contains { get; set; } = new List<string>();
        public List<string> NotContains { get; set; } = new List<string>();
        public string ExactMatch { get; set; }
        public JsonArray JsonArray { get; set; }
        public List<JsonProperty> JsonObject { get; set; }
        public PerfTarget PerfTarget { get; set; }
    }
}
