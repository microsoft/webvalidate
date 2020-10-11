// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;

namespace CSE.WebValidate
{
    /// <summary>
    /// Assembly Versioning
    /// </summary>
    public sealed class Version
    {
        // cache the assembly version
        private static string version = string.Empty;

        /// <summary>
        /// Gets assembly version
        /// </summary>
        public static string AssemblyVersion
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    if (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute v)
                    {
                        version = v.InformationalVersion;
                    }
                }

                return version;
            }
        }
    }
}