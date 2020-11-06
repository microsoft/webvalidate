// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We catch all exceptions for logging and return results appropriately")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Cosmos uses strings for URI")]
[assembly: SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "Cosmos uses strings for URI")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "log values are not localized")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "handled in IDispose", Scope = "type", Target = "~T:CSE.WebValidate.WebV")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "json serialization requires setter")]
