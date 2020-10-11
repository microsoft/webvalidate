// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace CSE.WebValidate
{
    /// <summary>
    /// WebV class (partial)
    /// </summary>
    public partial class WebV : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// iDisposable::Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        /// <param name="disposing">currently disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (client != null)
                {
                    client.Dispose();
                }
            }

            // Free any unmanaged objects
            disposed = true;
        }
    }
}
