using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BusLib.Helper
{
    public class SafeDisposableActions : SafeDisposable
    {
        //private bool _isDisposed = false;
        private ConcurrentStack<Action> _disposeActions = new ConcurrentStack<Action>();

        public SafeDisposableActions(Action disposeAction)
        {
            _disposeActions.Push(disposeAction);
        }

        public void Register(Action action)
        {
            _disposeActions.Push(action);
        }

        #region Cleanup

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Code to dispose the managed resources of the class
            }
            // Code to dispose the un-managed resources of the class
            var collection = Interlocked.Exchange(ref _disposeActions, null);
            if (collection != null)
            {
                foreach (var action in collection)
                {
                    Robustness.Instance.SafeCall(() => action?.Invoke());
                }
                collection.Clear();
            }

            
        }

        #endregion
    }
}