// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Represents a json property by array index
    /// </summary>
    public class JsonPropertyByIndex
    {
        /// <summary>
        /// gets or sets the array index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// gets or sets the json property name
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// gets or sets the value to check
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// gets or sets the recursive validation object
        /// </summary>
        public Validation Validation { get; set; }
    }
}
