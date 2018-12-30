//using BusLib.BatchEngineCore.Saga;
//using BusLib.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BusLib.BatchEngineCore.Handlers
//{
//    internal abstract class TaskExecutor
//    {
//        public abstract void Execute(TaskContext taskContext, ISerializer serializer);
//    }

//    class SimpleTaskExecutor : TaskExecutor
//    {
//        readonly ITask _task;

//        public SimpleTaskExecutor(ITask task)
//        {
//            _task = task;
//        }

//        public override void Execute(TaskContext taskContext, ISerializer serializer)
//        {
//            _task.Handle(taskContext, serializer);
//        }
//    }

//    //class SagaTaskExecutor<T> : TaskExecutor
//    //{
//    //    readonly ITaskSaga<T> _task;

//    //    public SagaTaskExecutor(ITaskSaga<T> task)
//    //    {
//    //        _task = task;
//    //    }

//    //    public override void Execute(ITaskContext taskContext, ISerializer serializer)
//    //    {
//    //        _task.Handle(taskContext, serializer);
//    //    }
//    //}

//}
