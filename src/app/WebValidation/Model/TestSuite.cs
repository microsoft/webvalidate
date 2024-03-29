// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSE.WebValidate.Model
{
    /// <summary>
    /// TestSuite Class - To report the XML results as TestSuite XML object
    /// </summary>
    public class TestSuite
    {
        /// <summary>
        /// Gets or sets Total number of failures in running the tests
        /// </summary>
        [XmlAttributeAttribute("failures")]
        public string Failures { get; set; }

        /// <summary>
        /// Gets or sets Name of the test suite
        /// </summary>
        [XmlAttributeAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Number of skipped tests, if the frameworks is capable to reporting
        /// </summary>
        [XmlAttributeAttribute("skipped")]
        public string Skipped { get; set; }

        /// <summary>
        /// Gets or sets Number of tests executed
        /// </summary>
        [XmlAttributeAttribute("tests")]
        public string Tests { get; set; }

        /// <summary>
        /// Gets or sets Total time taken in seconds to run the tests
        /// </summary>
        [XmlAttributeAttribute("time")]
        public string Time { get; set; }

        /// <summary>
        /// Gets or sets List of testcase objects to report each test run related values
        /// </summary>
        [XmlElementAttribute("testcase")]
        public List<TestCase> TestCases { get; set; }

        /// <summary>
        /// Serializes the TestSuite object to XML string
        /// </summary>
        /// <returns>A XML string</returns>
        public string ToXml()
        {
            System.IO.StringWriter stringWriter = new ();
            XmlSerializer serializer_name = new (typeof(TestSuite));
            serializer_name.Serialize(stringWriter, this);

            return stringWriter.ToString();
        }
    }
}
