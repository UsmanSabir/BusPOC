using System;

namespace BusLib.Helper
{
    public class SafeDisposable : IDisposable
    {
        protected bool IsDisposed=false;


        #region Cleanup

        ~SafeDisposable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Code to dispose the managed resources of the class
            }
            // Code to dispose the un-managed resources of the class
            
            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion
    }
}