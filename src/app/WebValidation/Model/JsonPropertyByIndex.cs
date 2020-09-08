namespace CSE.WebValidate.Model
{
    public class JsonPropertyByIndex
    {
        public int Index { get; set; }
        public string Field { get; set; }
        public object Value { get; set; }
        public Validation Validation { get; set; }
    }
}
