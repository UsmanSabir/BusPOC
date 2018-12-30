namespace BusLib.Infrastructure
{
    public interface IProcessDataStorage
    {
        void AddOrUpdateProcessData<T>(long processStateId, string key, T value);
        T GetProcessData<T>(long processStateId, string key);
        void CleanProcessData(string processId);
        bool IsHealthy();

        void RefreshIfNotHealth();
    }
}