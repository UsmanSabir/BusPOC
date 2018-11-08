using System.Collections.Generic;

namespace BusLib.BatchEngineCore
{
    public interface IBaseProcess<out T, out TU> where TU : ITask
    {
        IEnumerable<T> GetVolume(IProcessExecutionContext processContext);
    }

    public abstract class BaseProcess <T, TU>: IBaseProcess<T, TU>  where TU : ITask
    {
        public abstract IEnumerable<T> GetVolume(IProcessExecutionContext processContext);

        public virtual void TaskExecutionStarting(IProcessExecutionContext processContext)
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
}