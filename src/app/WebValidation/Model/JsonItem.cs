// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Represents a json item
    /// </summary>
    public class JsonItem
    {
        /// <summary>
        /// Gets or sets the name of the field
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the value to check
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets a recursive validation object
        /// </summary>
        public Validation Validation { get; set; }
    }
}
