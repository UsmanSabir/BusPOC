using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;

namespace BatchBootstrapper.Process
{
    public class TestCCProcess: StatelessProcess<int>, IHaveSupportingData
    {
        public override IEnumerable<int> GetVolume(IProcessExecutionContext processContext)
        {
            return Enumerable.Range(1, 50);
            //return null;
            //return Enumerable.Empty<int>();
        }

        public override int ProcessKey => 5301;
        
        
        public void InitializeSupportingData(IProcessExecutionContext context)
        {
            //context.AddUpdateProcessData("test_w", "Hello Cached World");
            //context.AddUpdateProcessData("test_w", "Hello Cached World- updated");
            //context.AddUpdateProcessData("test2_w", "I am back");
        }

        public override void Execute(int id, ITaskContext context)
        {
            context.Logger.Info("First step");
            var processData = context.ProcessExecutionContext.GetProcessData<string>("test_w");
            context.Logger.Info($"Data read from store= {processData}");

            
            //if (id % 5 == 0 && context.DeferredCount < 30)
            //{
            //    context.Logger.Info("Going to defer");
            //    var isDeferred = context.Defer();
            //    if (isDeferred)
            //    {
            //        context.Logger.Info($"Task deferred {context.State.DeferredCount}");
            //        return;
            //    }
            //    else
            //    {
            //        context.Logger.Warn($"Task deferred Failed {context.State.DeferredCount}");
            //    }
            //}
            //if (!context.IsRetry)
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(50);
                }

            //if (!context.IsRetry)
            //    throw new NotImplementedException();

            Thread.Sleep(100);
        }

        public override void ProcessCompleted(IProcessExecutionContext processContext)
        {
            base.ProcessCompleted(processContext);
        }

        public override void ProcessFinalizer(IProcessExecutionContext processContext)
        {
            //processContext.SetVolumeGenerated();
            processContext.Logger.Info("Completed");
            base.ProcessFinalizer(processContext);
        }

        public override void OnRetry(IProcessRetryContext processContext)
        {
            processContext.Logger.Info("OnRetry");
            base.OnRetry(processContext);
        }

        public override void ProcessStopped(IProcessStoppedContext stoppedContext)
        {
            stoppedContext.Logger.Info("OnRetry");
            base.ProcessStopped(stoppedContext);
        }
    }
}