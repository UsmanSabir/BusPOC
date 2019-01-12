using BusLib.BatchEngineCore.Handlers;
using BusLib.Core;

namespace BusLib.BatchEngineCore
{
    internal interface ITask
    {
        int ProcessKey { get; }

        ISerializer Serializer { get; }

        void Handle(TaskContext taskContext, ISerializer serializer, IStateManager stateManager);
    }

    internal interface ITask<in T, in TU>: ITask where TU: ITaskContext //<T>
    {
        //void Execute(T item, TU context);
    }

    public interface ITaskListener
    {
        void BeforeExecute(ITaskContext taskContext);

        void AfterExecute(ITaskContext taskContext);

        string Name { get;}
    }

    internal interface ITaskListenerHandler 
    {
        void InvokeBeforeExecute(ITaskContext taskContext);

        void InvokeAfterExecute(ITaskContext taskContext);

    }
}
