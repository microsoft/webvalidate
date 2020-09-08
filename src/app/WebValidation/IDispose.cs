using System;

namespace CSE.WebValidate
{
    public partial class WebV : IDisposable
    {
        private bool disposed = false;

        // iDisposable::Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
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
