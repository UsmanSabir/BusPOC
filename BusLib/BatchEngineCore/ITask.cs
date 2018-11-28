using BusLib.Core;

namespace BusLib.BatchEngineCore
{
    public interface ITask
    {
        int ProcessKey { get; }

        ISerializer Serializer { get; }

        void Handle(ITaskContext taskContext, ISerializer serializer);
    }

    public interface ITask<in T, in TU>: ITask where TU: ITaskContext //<T>
    {
        void Execute(T item, TU context);
    }

}
