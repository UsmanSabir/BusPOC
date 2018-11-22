using BusLib.BatchEngineCore.Volume;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BusLib.BatchEngineCore
{
    internal interface IBaseProcess
    {
        int ProcessKey { get; }

        Type VolumeDataType { get; }

        void HandleVolume(IVolumeHandler handler, IProcessExecutionContext processContext);
    }

    internal interface IBaseProcess<out T> : IBaseProcess
    {
        IEnumerable<T> GetVolume(IProcessExecutionContext processContext);

        //void LinkProcess<T1, T2>(IBaseProcess<T1, T2> process) where T2 : ITask;
    }

    internal interface IBaseProcessWithExecutor<out T, out TU> : IBaseProcess<T> where TU : ITask
    {
     
    }

    public abstract class BaseProcess <T, TU>: IBaseProcessWithExecutor<T, TU>  where TU : ITask
    {
        public abstract IEnumerable<T> GetVolume(IProcessExecutionContext processContext);
        public abstract int ProcessKey { get; }

        public Type VolumeDataType { get; } = typeof(T);
        public Type TaskActorType { get; } = typeof(TU);

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
    }

    public abstract class BaseTaskProcess<T> : BaseProcess<T, BaseTaskProcess<T>>, ITaskUnit<T>
    {
        public abstract void Execute(ITaskContext<T> context);
        public abstract override int ProcessKey { get; }
    }
}