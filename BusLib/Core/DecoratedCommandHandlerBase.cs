#define LOCK1
#define RWLOCK1

using System;
using System.Threading;
using BusLib.Helper;

namespace BusLib.Core
{
    public abstract class FeatureCommandHandlerBase<T> : SafeDisposable, IFeatureHandler<T> where T:IMessage
    {
        public FeatureCommandHandlerBase()
        {
            //Handler = handler;

            action = (command) => FeatureDecoratorHandler(command);
            //_queryAction = (cmd) => FeatureDecoratorQuery(cmd);
            IsEnabled = true;
        }

#if RWLOCK
        ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(); 
#endif

        public IHandler<T> Handler { get; private set; }

        public bool IsEnabled { get; private set; }

        Action<T> action;
        //private Func<T, TU> _queryAction;

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

        //public TResult Query<TResult>(T message)
        //{
        //    return FeatureDecoratorQuery<TResult>(message);
        //}

        
        public abstract void FeatureDecoratorHandler(T message);

        //public abstract TRes FeatureDecoratorQuery<TRes>(T message);
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

                //_queryAction = (cmd) => FeatureDecoratorQuery(cmd);

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
                //_queryAction = (cmd) => Handler.Query(cmd);

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
