namespace BusLib.BatchEngineCore
{
    public class NodeSettings
    {
        private static NodeSettings _instance;

        public static NodeSettings Instance => _instance ?? (_instance = new NodeSettings()); //todo

        public string Name { get; private set; } = "Node1"; //todo
        public int Throttling { get; private set; } = 0;
    }
}