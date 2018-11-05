using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRsConsole
{
    public class Message
    {
        public DateTime TimeStamp { get; protected set; }
        public string SagaId { get; protected set; }
    }
    public class Command : Message
    {
        public string Name { get; protected set; }
    }
    public class Event : Message
    {
        // Any properties that may help retrieving
        // and persisting events.
    }
}
