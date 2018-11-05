using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Messages
{
    public class SystemFeatureToggleCommand : ISystemCommand
    {
        public SystemFeatureToggleCommand(string pipeLineKey, Type featureType, bool enable)
        {
            PipeLineKey = pipeLineKey;
            FeatureHandlerType = featureType;
            Enable = enable;
        }

        public string PipeLineKey { get; private set; } // => nameof(ICommand);

        public Type FeatureHandlerType { get; private set; }
        public bool Enable { get; private set; }
    }
}
