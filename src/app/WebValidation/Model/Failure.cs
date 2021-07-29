// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Xml.Serialization;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// Failure message - if multiple errors are reported - they are joined as single failure message
    /// </summary>
    public class Failure
    {
        /// <summary>
        /// Message - rpeorting all errors rolled in as a single string
        /// </summary>
        [XmlAttributeAttribute("message")]
        public string Message { get; set; }
    }
}
