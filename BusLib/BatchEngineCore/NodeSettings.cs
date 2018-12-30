using System.Threading;

namespace BusLib.BatchEngineCore
{
    public class NodeSettings
    {
        private static NodeSettings _instance;

        public NodeSettings()
        {
            LockKey = "BPEM" + System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        public static NodeSettings Instance => _instance ?? (_instance = new NodeSettings()); //todo

        public string Name { get; private set; } = "Node1"; //todo
        public int Throttling { get; private set; } = 1;
        public string LockKey { get; private set; }
    }
}