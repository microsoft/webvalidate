// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Performance target class
    /// </summary>
    public class PerfTarget
    {
        /// <summary>
        /// gets or sets the category name
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// gets or sets a list of ms to determine quartile
        /// list should contain exactly 3 values
        /// </summary>
        public List<double> Quartiles { get; set; }
    }
}
