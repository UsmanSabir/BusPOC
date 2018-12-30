//using System.Collections.Generic;
//using BusLib.BatchEngineCore;
//using BusLib.Core;

//namespace CQRsConsole.TestProcess
//{
//    public class UnitProcessWithTask:BaseProcess<int, UnitProcessWithTask>, ITaskUnit<int>
//    {
//        public override IEnumerable<int> GetVolume(IProcessExecutionContext processContext)
//        {
//            throw new System.NotImplementedException();
//        }

//        public override int ProcessKey { get; } = 1;

//        public ISerializer Serializer { get; } = null;

//        public void Execute(int item, ITaskContext context)
//        {
//            throw new System.NotImplementedException();
//        }

//        public void Handle(ITaskContext taskContext, ISerializer serializer)
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//}