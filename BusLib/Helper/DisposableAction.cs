using System;
using System.Threading;

namespace BusLib.Helper
{
    public class DisposableAction:IDisposable
    {
        private Action _disposeAction;

        public DisposableAction(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            Robustness.Instance.SafeCall(()=> Interlocked.Exchange(ref _disposeAction, null)?.Invoke());
        }
    }
}