using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    public class JsonArray
    {
        public int? Count { get; set; }
        public int? MinCount { get; set; }
        public int? MaxCount { get; set; }
        public List<Validation> ForEach { get; set; }
        public List<JsonPropertyByIndex> ByIndex { get; set; }
    }
}
