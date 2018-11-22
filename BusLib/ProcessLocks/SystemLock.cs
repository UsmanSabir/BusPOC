using System;
using System.Threading;
using BusLib.Core;

namespace BusLib.ProcessLocks
{
    public class SystemLock:ILock
    {
        private EventWaitHandle _event;

        public SystemLock(EventWaitHandle @event)
        {
            _event = @event;
        }

        void IDisposable.Dispose()
        {
            var @event = Interlocked.Exchange(ref this._event, null);
            if (@event != null)
            {
                @event.Set(); // signal
                @event.Dispose();
            }
        }
    }
}