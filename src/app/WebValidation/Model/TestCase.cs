// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Xml.Serialization;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// TestCase Class to To report the individual test run XML results as TestCase object
    /// </summary>
    public class TestCase
    {
        /// <summary>
        /// Gets or sets Classname - combination of tag:verb:Path
        /// </summary>
        [XmlAttributeAttribute("classname")]
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets Name - combination of tag:verb:Path
        /// </summary>
        [XmlAttributeAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Time - Total time taken in seconds to run this test
        /// </summary>
        [XmlAttributeAttribute("time")]
        public string Time { get; set; }

        /// <summary>
        /// Gets or sets Failure message
        /// </summary>
        [XmlElementAttribute("failure")]
        public Failure Failure { get; set; }

        /// <summary>
        /// Gets or sets SystemOut - Variable with low significance but needed for report rendering in Azure Devops
        /// </summary>
        [XmlElementAttribute("system-out")]
        public string SystemOut { get; set; }
    }
}
