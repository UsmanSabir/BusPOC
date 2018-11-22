using System.Runtime.Caching;
using BusLib.BatchEngineCore;

namespace BusLib.Helper
{
    /// <summary>
    /// DataStore to avoid repeated access data source
    /// </summary>
    public interface ICacheAside
    {
        //MemoryCache.Default;  //20 mins expiry of pool
        IProcessExecutionContext GetProcessExecutionContext(int processId);

        ProcessConfiguration GetProcessConfiguration(int processKey);

        object GetProcessData(int processId, string dataKey); // might be from central cache, a dictionary of string,object type to store data

        T GetProcessData<T>(int processId, string dataKey); // might be from central cache
    }
}