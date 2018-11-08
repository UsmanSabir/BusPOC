namespace BusLib.BatchEngineCore
{
    public interface ITask
    {

    }

    public interface ITask<in T, in TU>: ITask where TU: ITaskContext<T>
    {
        void Execute(TU context);
    }

}
