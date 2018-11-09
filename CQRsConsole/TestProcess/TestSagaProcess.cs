using System.Collections.Generic;
using BusLib.BatchEngineCore;

namespace CQRsConsole.TestProcess
{
    public class TestSagaProcess : BaseProcess<TestDataContext, TestSagaTask>
    {
        public override IEnumerable<TestDataContext> GetVolume(IProcessExecutionContext processContext)
        {
            throw new System.NotImplementedException();
        }
    }
}