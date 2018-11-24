using BusLib.Core;

namespace BusLib.BatchEngineCore
{
    public interface ITask
    {
        int ProcessKey { get; }
    }

    public interface ITask<in T, in TU>: ITask where TU: ITaskContext<T>
    {
        void Execute(TU context);
    }

}
