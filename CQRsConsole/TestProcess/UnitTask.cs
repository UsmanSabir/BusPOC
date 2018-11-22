using BusLib.BatchEngineCore;

namespace CQRsConsole.TestProcess
{
    public class UnitTask:ITaskUnit<int>
    {
        public void Execute(ITaskContext<int> context)
        {
            throw new System.NotImplementedException();
        }

        public int ProcessKey { get; } = 1;
    }
}