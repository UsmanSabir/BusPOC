using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Saga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRsConsole.TestProcess
{
    public class TestSagaTask : BaseSagaTask<TestDataContext>
    {

        public TestSagaTask()
        {
            DefineState("CheckOrderProducts", CheckOrder);
            DefineState("Prepare", PrepareOrder);
            DefineState("CompleteOrder", CompleteOrder);
        }

        public override void Started(ITaskContext<TestDataContext> context)
        {
            throw new NotImplementedException();
        }

        public override int ProcessKey { get; } = 1;

        private void CompleteOrder(ITaskContext<TestDataContext> obj)
        {
            throw new NotImplementedException();
        }

        private void PrepareOrder(ITaskContext<TestDataContext> obj)
        {
            throw new NotImplementedException();
        }

        private void CheckOrder(ITaskContext<TestDataContext> obj)
        {
            throw new NotImplementedException();
        }
    }
}
