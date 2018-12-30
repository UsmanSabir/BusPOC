//using BusLib.BatchEngineCore;
//using BusLib.BatchEngineCore.Saga;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CQRsConsole.TestProcess
//{
//    public class TestSagaTask : BaseSagaTask<TestDataContext>
//    {

//        public TestSagaTask()
//        {
//            DefineState("CheckOrderProducts", CheckOrder);
//            DefineState("Prepare", PrepareOrder);
//            DefineState("CompleteOrder", CompleteOrder);
//        }

//        public override void Started(TestDataContext item, ITaskContext context)
//        {
//            throw new NotImplementedException();
//        }

//        public override int ProcessKey { get; } = 1;

//        private void CompleteOrder(TestDataContext item, ITaskContext obj)
//        {
//            throw new NotImplementedException();
//        }

//        private void PrepareOrder(TestDataContext item, ITaskContext obj)
//        {
//            throw new NotImplementedException();
//        }

//        private void CheckOrder(TestDataContext item, ITaskContext obj)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
