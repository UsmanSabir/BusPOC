using System;
using System.Collections.Generic;

namespace BusLib.BatchEngineCore.PubSub
{
    internal class HealthMessage: IBroadcastMessage
    {
        public HealthMessage()
        {
            Matrix= new List<HealthBlock>();
        }

        public Guid Id { get; set; }
        public DateTime Time { get; set; }
        public string NodeKey { get; set; }
        public int NodeThrottling { get; set; }
        public object Sender { get; set; }

        public List<HealthBlock> Matrix { get; set; }
    }

    class HealthBlock
    {
        public HealthBlock()
        {
            Matrix=new List<KeyValuePair<string, object>>();
            //Details=new List<HealthBlock>();
        }
        public string Name { get; set; }

        public List<KeyValuePair<string, object>> Matrix { get; set; }
        public List<HealthBlock> Details { get; set; }
    }

    internal class PingHealthMessage:IBroadcastMessage
    {
        public string NodeKey { get; set; }
        public bool IsWorking { get; set; }
        public object Sender { get; set; }
    }
}