// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Represents a json array
    /// </summary>
    public class JsonArray
    {
        /// <summary>
        /// gets or sets the array count
        /// default: null (ignore)
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// gets or sets the minimum array count
        /// default: null (ignore)
        /// </summary>
        public int? MinCount { get; set; }

        /// <summary>
        /// gets or sets the maximum array count
        /// default: null (ignore)
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// gets or sets the list of validations for each array object
        /// default: null (ignore)
        /// </summary>
        public List<Validation> ForEach { get; set; }

        /// <summary>
        /// gets or sets the list of validations for any array object
        /// default: null (ignore)
        /// </summary>
        public List<Validation> ForAny { get; set; }

        /// <summary>
        /// gets or sets the validation for a specific array object by array index
        /// default: null (ignore)
        /// </summary>
        public List<JsonPropertyByIndex> ByIndex { get; set; }
    }
}
