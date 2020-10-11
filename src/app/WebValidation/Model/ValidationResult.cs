// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Validation Result class
    /// Contains the results of a validation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// gets or sets a value indicating whether the validation failed
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// gets a list of validation errors
        /// </summary>
        public List<string> ValidationErrors { get; } = new List<string>();

        /// <summary>
        /// adds a validation result to the collection
        /// </summary>
        /// <param name="result">validation result</param>
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
