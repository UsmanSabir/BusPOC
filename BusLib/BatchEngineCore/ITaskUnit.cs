using BusLib.BatchEngineCore.Saga;

namespace BusLib.BatchEngineCore
{
    internal interface ITaskUnit<in T> : ITask<T, ITaskContext>
    {
        
    }

}
