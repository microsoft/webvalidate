using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    public class ValidationResult
    {
        public bool Failed { get; set; } = false;

        public List<string> ValidationErrors { get; } = new List<string>();

        public void Add(ValidationResult result)
        {
            if (result != null)
            {
                if (result.ValidationErrors != null && result.ValidationErrors.Count > 0)
                {
                    ValidationErrors.AddRange(result.ValidationErrors);
                }

                if (result.Failed)
                {
                    Failed = true;
                }
            }
        }
    }
}
