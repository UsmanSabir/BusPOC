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

        public override void Started(ISagaTaskContext<TestDataContext> context)
        {
            throw new NotImplementedException();
        }

        public override int ProcessKey { get; } = 1;

        private void CompleteOrder(ISagaTaskContext<TestDataContext> obj)
        {
            throw new NotImplementedException();
        }

        private void PrepareOrder(ISagaTaskContext<TestDataContext> obj)
        {
            throw new NotImplementedException();
        }

        private void CheckOrder(ISagaTaskContext<TestDataContext> obj)
        {
            throw new NotImplementedException();
        }
    }
}
