using System.Collections.Generic;
using BusLib.BatchEngineCore;

namespace CQRsConsole.TestProcess
{
    public class UnitProcessWithTask:BaseProcess<int, UnitProcessWithTask>, ITaskUnit<int>
    {
        public override IEnumerable<int> GetVolume(IProcessExecutionContext processContext)
        {
            throw new System.NotImplementedException();
        }

        public void Execute(ITaskContext<int> context)
        {
            throw new System.NotImplementedException();
        }
    }
}