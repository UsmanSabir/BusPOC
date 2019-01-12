using System.Configuration;
using System.Threading;

namespace BusLib.BatchEngineCore
{
    public class NodeSettings
    {
        private static NodeSettings _instance;

        public NodeSettings()
        {
            LockKey = "BatchEngine"; // + System.Diagnostics.Process.GetCurrentProcess().Id
            var throttlingSettings = ConfigurationManager.AppSettings["Throttling"];
            if (!string.IsNullOrWhiteSpace(throttlingSettings) &&
                int.TryParse(throttlingSettings, out int throttling) && throttling >= 0)
            {
                Throttling = throttling;
            }
        }

        public static NodeSettings Instance => _instance ?? (_instance = new NodeSettings()); //todo

        public string Name { get; private set; } = ConfigurationManager.AppSettings["NodeId"]?? "Node"; //todo
        public int Throttling { get; private set; } = 0;
        public string LockKey { get; private set; }
    }
}