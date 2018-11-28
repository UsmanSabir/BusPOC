using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore.Volume
{
    public interface IVolumeHandler
    {
        //persist volume
        void Handle<T>(IEnumerable<T> volume, IProcessExecutionContext processContext);

        ITaskState GetNextTaskWithTransaction(out ITransaction transaction);

        
    }
}
