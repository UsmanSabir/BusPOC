using BusLib.BatchEngineCore;

namespace BusLib.Helper
{
    public interface ICacheAside
    {
        IProcessExecutionContext GetProcessExecutionContext(int processId);

        ProcessConfiguration GetProcessConfiguration(int processKey);
    }
}