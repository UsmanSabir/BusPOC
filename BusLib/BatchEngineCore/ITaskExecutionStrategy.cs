namespace BusLib.BatchEngineCore
{
    public interface ITaskExecutionStrategy
    {
        // singlton/ instance per task/ per process / saga / etl
        void ExecuteLogic(); // todo
    }
}