using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BusLib.Helper
{
    public class SafeDisposable : IDisposable
    {
        //private bool _isDisposed = false;
        private ConcurrentQueue<Action> _disposeAction = new ConcurrentQueue<Action>();

        public SafeDisposable(Action disposeAction)
        {
            _disposeAction.Enqueue(disposeAction);
        }

        public void OnCleanup(Action action)
        {
            _disposeAction.Enqueue(action);
        }

        #region Cleanup

        ~SafeDisposable()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Code to dispose the managed resources of the class
            }
            // Code to dispose the un-managed resources of the class
            var queue = Interlocked.Exchange(ref _disposeAction, null);
            if (queue != null)
            {
                foreach (var action in queue)
                {
                    Robustness.Instance.SafeCall(() => action?.Invoke());
                }
            }

            //_isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion
    }
}