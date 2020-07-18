namespace CSE.WebValidate.Model
{
    public class JsonProperty
    {
        public string Field { get; set; }
        public object Value { get; set; }
        public Validation Validation { get; set; }
    }
}
