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
        private static string shortVersion = string.Empty;

        /// <summary>
        /// Gets assembly version
        /// </summary>
        public static string AssemblyVersion
        {
            get
            {
                SetVersion();

                return version;
            }
        }

        /// <summary>
        /// Gets assembly version
        /// </summary>
        public static string ShortVersion
        {
            get
            {
                SetVersion();

                return shortVersion;
            }
        }

        /// <summary>
        /// Gets assembly version
        /// </summary>
        private static void SetVersion()
        {
            if (string.IsNullOrEmpty(version))
            {
                if (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute v)
                {
                    version = v.InformationalVersion;
                    shortVersion = version;

                    if (version.Contains('-', StringComparison.OrdinalIgnoreCase))
                    {
                        shortVersion = version.Substring(0, version.IndexOf('-', StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
        }
    }
}
