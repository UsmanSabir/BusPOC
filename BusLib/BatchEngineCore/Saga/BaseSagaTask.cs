//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BusLib.BatchEngineCore.Exceptions;
//using BusLib.Core;
//using BusLib.Helper;

//namespace BusLib.BatchEngineCore.Saga
//{
//    public abstract class BaseSagaTask<T> : ITaskSaga<T>
//    {
//        private readonly ConcurrentDictionary<string, Action<T, ITaskContext>> _sagaStateDictionary =
//            new ConcurrentDictionary<string, Action<T, ITaskContext>>();

//        protected BaseSagaTask()
//        {
//            //DefineSagaStates();
//        }

//        // protected abstract void DefineSagaStates();

//        protected void DefineState(string name, Action<T, ITaskContext> action)
//        {
//            _sagaStateDictionary.AddOrUpdate(name, action, (s,a)=> action);            
//        }

//        protected internal virtual string GetStateAction(ITaskContext context)
//        {
//            if (!string.IsNullOrWhiteSpace(context.NextState) && context.State.CurrentState!=context.NextState)
//                return context.NextState;

//            var nextState = _sagaStateDictionary.Keys.SkipWhile(s => s != context.State.CurrentState).ElementAtOrDefault(1);
//            return nextState;
//        }

//        public virtual void Started(T item, ITaskContext context)
//        {
            
//        }

//        public virtual void Completed(T item, ITaskContext context)
//        {
            
//        }

//        public void Execute(T item, ITaskContext context)
//        {
//            Started(item, context);
//        }

//        public void Handle(ITaskContext taskContext, ISerializer serializer)
//        {
//            var item = serializer.DeserializeFromString<T>(taskContext.State.Payload);
//            var state = taskContext.State.CurrentState;

//            Action<T, ITaskContext> action=null;

//            _sagaStateDictionary.TryGetValue(state, out action);

//            if (action==null) //string.IsNullOrWhiteSpace(state) && 
//            {
//                action = Execute; // (item, taskContext);
//            }

//            do
//            {
//                taskContext.Logger.Trace($"Excution started with state '{state}'");

//                action(item, taskContext);

//                taskContext.Logger.Trace($"Excution finished with state '{state}'");

//                state = GetStateAction(taskContext);

//                if(string.IsNullOrWhiteSpace(state) || state == taskContext.State.CurrentState)
//                {
//                    taskContext.MarkTaskStatus(TaskCompletionStatus.Finished, ResultStatus.Success, Constants.ReasonCompleted);
//                    return;
//                }

//                if(!_sagaStateDictionary.TryGetValue(state, out var nextAction) || nextAction == null)
//                {
//                    string errMsg = $"Saga state '{state}' not found";
//                    taskContext.Logger.Error(errMsg);
//                    throw new SagaStateException(errMsg);
//                }

//                if (ReferenceEquals(action, nextAction)) //string.IsNullOrWhiteSpace(state) && 
//                {
//                    string errMsg = $"Saga state '{state}' re-entering to previous state";
//                    taskContext.Logger.Error(errMsg);
//                    throw new SagaStateException(errMsg);
//                }
//                action = nextAction;

//            } while (taskContext.IsFinished());



//        }

//        public abstract int ProcessKey { get; }

//        public ISerializer Serializer => null;
//    }
//}
