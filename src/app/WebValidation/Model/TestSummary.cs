// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Xml.Serialization;

namespace CSE.WebValidate
{
    /// <summary>
    /// Class for displaying test summary
    /// </summary>
    public class TestSummary
    {
        /// <summary>
        /// gets or sets the number of validation errors
        /// </summary>
        public int ValidationErrorCount { get; set; }

        /// <summary>
        /// gets or sets the number of errors
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// gets or sets the max errors from config
        /// </summary>
        public int MaxErrors { get; set; }

        /// <summary>
        /// Write this object to the console in xml format
        /// </summary>
        public void WriteXmlToConsole()
        {
            XmlSerializer xs = new XmlSerializer(GetType());

            xs.Serialize(Console.Out, this);
            Console.WriteLine();
        }
    }
}
