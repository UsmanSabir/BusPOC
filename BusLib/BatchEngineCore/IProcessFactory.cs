using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    interface IProcessFactory
    {
        IBaseProcess GetProcess(string processKey);

        ProcessConfiguration GetProcessConfiguration(string processKey);
    }
}
