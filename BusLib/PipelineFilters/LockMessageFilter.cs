using System;
using BusLib.BatchEngineCore;
using BusLib.Core;

namespace BusLib.PipelineFilters
{
    public class LockMessageFilter<T> : FeatureCommandHandlerBase<T> where T: IExecutionContextMessage
    {
        private readonly Func<T, string> _keySelector;
        private readonly ILockFactory _lockFactory;
        private readonly int _lockWaitMillis;
        
        public LockMessageFilter(Func<T,string> keySelector, ILockFactory lockFactory, int lockWaitMillis)
        {
            _keySelector = keySelector;
            _lockFactory = lockFactory;
            _lockWaitMillis = lockWaitMillis;
        }        

        public override void FeatureDecoratorHandler(T context) 
        {
            //Trace message
            context.Logger.Trace("Acquiring lock");
            try
            {
                using (_lockFactory.AcquireLock(_keySelector(context), _lockWaitMillis))
                {
                    context.Logger.Trace("Lock acquired, passing message to next filter");
                    //trace message Lock Acquired

                    Handler?.Handle(context);

                    context.Logger.Trace("Lock releasing");
                }
            }
            catch (Exception e)
            {
                context.Logger.Error("ERROR while acquiring lock, Item skipped.", e);
            }
            
        }

        
    }
}