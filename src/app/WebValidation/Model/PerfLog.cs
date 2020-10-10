using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CSE.WebValidate.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
    public class PerfLog
    {
        public static string Type => "request";
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int StatusCode { get; set; }
        public bool Failed { get; set; }
        public bool Validated { get; set; } = true;
        public int ErrorCount => Errors == null ? 0 : Errors.Count;
        public double Duration { get; set; }
        public long ContentLength { get; set; }
        public string Category { get; set; }
        public int? Quartile { get; set; }
        public string Tag { get; set; }
        public string Path { get; set; }
        public List<string> Errors { get; set; }

        public string ToJson(bool verboseErrors)
        {
            System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (verboseErrors || Errors == null)
            {
                return System.Text.Json.JsonSerializer.Serialize(this, options);
            }

            // don't serialize the errors
            string json = System.Text.Json.JsonSerializer.Serialize(this, options);

            int i = json.IndexOf(",\"errors\":", StringComparison.Ordinal);

            // remove the error messages
            if (i > -1)
            {
                json = json.Substring(0, i) + "}";
            }

            return json;
        }
    }
}
