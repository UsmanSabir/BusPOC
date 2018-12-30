using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BusLib.BatchEngineCore.Exceptions;
using BusLib.BatchEngineCore.Handlers;
using BusLib.BatchEngineCore.Saga;
using BusLib.Helper;

namespace BusLib.BatchEngineCore
{
    public interface IBaseProcess
    {
        int ProcessKey { get; }

        Type VolumeDataType { get; }

        void HandleVolume(IVolumeHandler handler, IProcessExecutionContext processContext);
    }

    public interface IBaseProcess<out T> : IBaseProcess
    {
        IEnumerable<T> GetVolume(IProcessExecutionContext processContext);

        //void LinkProcess<T1, T2>(IBaseProcess<T1, T2> process) where T2 : ITask;
    }

    internal interface IBaseProcessWithExecutor<out T, out TU> : IBaseProcess<T> where TU : ITask
    {
     
    }

    public interface IHaveSupportingData
    {
        void InitializeSupportingData(IProcessExecutionContext context);
    }

    public abstract class BaseProcess <T>: IBaseProcess<T>  //where TU : ITask
    {
        public abstract IEnumerable<T> GetVolume(IProcessExecutionContext processContext);
        public abstract int ProcessKey { get; }

        public Type VolumeDataType { get; } = typeof(T);
        //public Type TaskActorType { get; } = typeof(TU);

        protected bool CanRegenerateVolume { get; set; } = false;

        public virtual void ProcessStarting(IProcessExecutionContext processContext)
        {

        }

        public virtual void ProcessCompleted(IProcessExecutionContext processContext)
        {

        }

        public virtual void ProcessFailed(IProcessExecutionContext processContext)
        {

        }

        public virtual void ProcessFinalizer(IProcessExecutionContext processContext)
        {

        }

        void IBaseProcess.HandleVolume(IVolumeHandler handler, IProcessExecutionContext processContext)
        {
            var volumeGenerated = processContext.ProcessState.IsVolumeGenerated;
            if (!volumeGenerated || CanRegenerateVolume)
            {
                handler.Handle(GetVolume(processContext), processContext); //volume visitor
            }            

        }

        public ISerializer Serializer { get; protected set; }
    }

    public abstract class BaseTaskProcess<T> : BaseProcess<T>
    {
        //public abstract void Execute(T item, ITaskContext context);
        
        public abstract override int ProcessKey { get; }

        
        
    }

    public abstract class StatelessProcess<T> : BaseTaskProcess<T> , ITask<T, TaskContext>
    {
        public abstract void Execute(T item, ITaskContext context);

        //void ITask<T, TaskContext>.Execute(T item, TaskContext context)
        //{
        //    throw new NotImplementedException();
        //}

        void ITask.Handle(TaskContext taskContext, ISerializer serializer)
        {
            var item = serializer.DeserializeFromString<T>(taskContext.State.Payload);
            taskContext.Logger.Info("Task started");
            taskContext.MarkTaskStarted();

            Execute(item, taskContext);
        }
    }

    public abstract class StatefulProcess<T> : BaseTaskProcess<T>, ITaskSaga<T>
    {
        //private readonly ConcurrentDictionary<string, Action<T, ITaskContext>> _sagaStateDictionary =
        //    new ConcurrentDictionary<string, Action<T, ITaskContext>>();

        private readonly LinkedList<(string name, Action<T, ITaskContext> action)> _sagaStateDictionary =new LinkedList<(string name, Action<T, ITaskContext> action)>();
        
        

        public StatefulProcess()
        {
            DefineState(ResultStatus.Empty.Name, Started);
            InitializeStates();
        }
        
        public abstract void InitializeStates();

        protected void DefineState(string name, Action<T, ITaskContext> action)
        {
            //_sagaStateDictionary.AddOrUpdate(name, action, (s, act) => action);

            _sagaStateDictionary.AddLast((name, action));
        }


        protected internal virtual (string name, Action<T, ITaskContext> action)? GetStateAction(ITaskContext context,
            out bool isFirst, bool moveNext = false)
        {
            isFirst = true;
            var stat = context.State.CurrentState;
            if (!string.IsNullOrWhiteSpace(context.NextState) && context.State.CurrentState != context.NextState)
                stat = context.NextState;

            if (string.IsNullOrWhiteSpace(stat))
            {
                isFirst = true;
                return _sagaStateDictionary.First?.Value;
            }

            var current = _sagaStateDictionary.First;
            while (current != null && current.Value.name!=stat)
            {
                current = current.Next;
                isFirst = false;
            }

            if (moveNext)
            {
                current = current?.Next;
                isFirst = false;
            }
            var next = current?.Value;
            return next;
            //var nextState = _sagaStateDictionary.Keys.SkipWhile(s => s != context.State.CurrentState).ElementAtOrDefault(1);
            //return nextState;
        }

        public virtual void Started(T item, ITaskContext context)
        {

        }
        
        public virtual void Completed(T item, ITaskContext context)
        {

        }

        //public void Execute(T item, ITaskContext context)
        //{
        //    Started(item, context);
        //}

        void ITask.Handle(TaskContext taskContext, ISerializer serializer)
        {
            var item = serializer.DeserializeFromString<T>(taskContext.State.Payload);
            //var state = taskContext.State.CurrentState;

            //Action<T, ITaskContext> action = null;

            var stateAction = GetStateAction(taskContext, out bool isFirst);

            //_sagaStateDictionary.TryGetValue(state, out action);

            if (stateAction == null) //string.IsNullOrWhiteSpace(state) && 
            {
                taskContext.Logger.Warn($"No action found from state {taskContext.State.CurrentState??string.Empty}. Setting started");
                stateAction = (ResultStatus.Empty.Name, this.Started);
                // action = this.Started; // (item, taskContext);
                //state = ResultStatus.Empty.Name;
            }
            var action = stateAction.Value.action;
            var state = stateAction.Value.name;

            do
            {
                if (isFirst)
                {
                    taskContext.Logger.Info("Task started");
                    taskContext.MarkTaskStarted();
                }

                taskContext.Logger.Trace($"State '{state}' execution started");

                action(item, taskContext);

                taskContext.Logger.Trace($"State '{state}' execution finished");

                if (taskContext.IsDeferred)
                {
                    taskContext.Logger.Trace($"Task deferred on state '{state}' with deferred count {taskContext.State.DeferredCount}");
                    return;
                }

                //state = GetStateAction(taskContext);
                stateAction = GetStateAction(taskContext, out isFirst, true);
                var nextAction = stateAction?.action;
                state = stateAction?.name;

                if (string.IsNullOrWhiteSpace(state) || state == taskContext.State.CurrentState)
                {
                    taskContext.PreserveNextState(state, Constants.ReasonCompleted);
                    //taskContext.MarkTaskStatus(CompletionStatus.Finished, ResultStatus.Success, Constants.ReasonCompleted);
                    Robustness.Instance.SafeCall(()=>Completed(item, taskContext), taskContext.Logger,"Error in Completed action. {0}");

                    return;
                }

                taskContext.PreserveNextState(state);

                //if (!_sagaStateDictionary.TryGetValue(state, out var nextAction) || nextAction == null)
                //{
                //    string errMsg = $"Saga state '{state}' not found";
                //    taskContext.Logger.Error(errMsg);
                //    throw new SagaStateException(errMsg);
                //}

                if (ReferenceEquals(action, nextAction) || nextAction==null) //string.IsNullOrWhiteSpace(state) && 
                {
                    string errMsg = $"Saga state '{state}' re-entering to previous state";
                    taskContext.Logger.Error(errMsg);
                    throw new SagaStateException(errMsg);
                }
                action = nextAction;

            } while (!taskContext.IsFinished());



        }

        //void ITask<T, TaskContext>.Execute(T item, TaskContext context)
        //{
        //    Started(item, context);
        //}

        
    }


}