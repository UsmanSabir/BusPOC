using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.BatchEngineCore;

namespace BatchBootstrapper.Process
{
    public class TestDDProcess: StatefulProcess<int>, IHaveSupportingData
    {
        public override IEnumerable<int> GetVolume(IProcessExecutionContext processContext)
        {
            return Enumerable.Range(1, 500);
        }

        public override int ProcessKey => 5301;
        
        public override void InitializeStates()
        {
            DefineState("Started", FirstStep);
            DefineState("Second", SecondStep);
        }
        
        private void FirstStep(int id, ITaskContext context)
        {
            context.Logger.Info("First step");
            var processData = context.ProcessExecutionContext.GetProcessData<string>("test");
            context.Logger.Info($"Data read from store= {processData}");

            
            if (id % 5==0 && context.DeferredCount<30)
            {
                context.Logger.Info("Going to defer");
                var isDeferred = context.Defer();
                if (isDeferred)
                {
                    context.Logger.Info($"Task deferred {context.State.DeferredCount}");
                    return;
                }
                else
                {
                    context.Logger.Warn($"Task deferred Failed {context.State.DeferredCount}");
                }
            }

            Thread.Sleep(2000);

            bool stateSet = context.SetNextState("Second");
            context.Logger.Info($"Next state set {stateSet} val = {context.NextState}");

        }

        private void SecondStep(int id, ITaskContext context)
        {
            context.Logger.Info("Second step");
            Thread.Sleep(2000);
        }

        public void InitializeSupportingData(IProcessExecutionContext context)
        {
            context.AddUpdateProcessData("test", "Hello Cached World");
            context.AddUpdateProcessData("test", "Hello Cached World- updated");
            context.AddUpdateProcessData("test2", "I am back");
        }
    }
}