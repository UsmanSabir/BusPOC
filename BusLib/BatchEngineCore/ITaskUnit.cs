using BusLib.BatchEngineCore.Saga;

namespace BusLib.BatchEngineCore
{
    public interface ITaskUnit<in T> : ITask<T, ITaskContext<T>>
    {
        
    }

}
