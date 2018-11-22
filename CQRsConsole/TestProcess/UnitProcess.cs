using System.Collections.Generic;
using BusLib.BatchEngineCore;

namespace CQRsConsole.TestProcess
{
    public class UnitProcess:BaseProcess<int, UnitTask>
//<int, >
    {
        public override IEnumerable<int> GetVolume(IProcessExecutionContext processContext)
        {
            throw new System.NotImplementedException();
        }

        public override int ProcessKey { get; } = 1;
    }
}