#define LOCK1
#define RWLOCK1

using System;
using System.Threading;

namespace BusLib.Core
{
    public abstract class FeatureCommandHandlerBase<T> : IFeatureHandler<T> where T:IMessage
    {
        public FeatureCommandHandlerBase()
        {
            //Handler = handler;

            action = (command) => FeatureDecoratorHandler(command);
            IsEnabled = true;
        }

#if RWLOCK
        ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(); 
#endif

        public IHandler<T> Handler { get; private set; }

        public bool IsEnabled { get; private set; }

        Action<T> action;

        public void Handle(T message)
        {
#if RWLOCK
            _locker.EnterReadLock();
#endif

#if LOCK
            lock (_lock)
#endif
            {
                action(message); 
            }
#if RWLOCK
            _locker.ExitReadLock(); 
#endif
        }

        public abstract void FeatureDecoratorHandler(T message);
#if LOCK

        object _lock = new object();

#endif
        public void Enable()
        {
            //todo reader/writer lock
#if LOCK
            lock (_lock)

#endif
#if RWLOCK
            _locker.EnterWriteLock(); 
#endif
            {
                action = (message) => FeatureDecoratorHandler(message);
                IsEnabled = true;
            }
#if RWLOCK
            _locker.ExitWriteLock(); 
#endif
        }

        public void Disable()
        {
#if LOCK
            lock (_lock)

#endif
#if RWLOCK
            _locker.EnterWriteLock(); 
#endif
            {
                action = (message) => Handler?.Handle(message);
                IsEnabled = false;
            }
#if RWLOCK
            _locker.ExitWriteLock(); 
#endif
        }

        public IFeatureHandler<T> Register(IHandler<T> handler)
        {
            this.Handler = handler;
            return this;
        }
    }
}
