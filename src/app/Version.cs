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
        static string version = string.Empty;

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