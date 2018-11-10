using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BusLib.BatchEngineCore
{
    public interface IBaseProcess
    {
        Type VolumeType { get; }
    }
    public interface IBaseProcess<out T, out TU> : IBaseProcess where TU : ITask
    {
        IEnumerable<T> GetVolume(IProcessExecutionContext processContext);

        //void LinkProcess<T1, T2>(IBaseProcess<T1, T2> process) where T2 : ITask;
    }

    public abstract class BaseProcess <T, TU>: IBaseProcess<T, TU>  where TU : ITask
    {
        public abstract IEnumerable<T> GetVolume(IProcessExecutionContext processContext);
        public Type VolumeType { get; } = typeof(T);
        public Type TaskActorType { get; } = typeof(TU);


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


    }

    public abstract class BaseTaskProcess<T> : BaseProcess<T, BaseTaskProcess<T>>, ITaskUnit<T>
    {
        public abstract void Execute(ITaskContext<T> context);
    }
}