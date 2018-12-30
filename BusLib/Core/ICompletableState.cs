using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface ICompletableState
    {
        bool IsFinished { get; }
        bool IsStopped { get; }
    }

    public interface IWritableCompletableState
    {
        bool IsFinished { set; }
        bool IsStopped { set; }
    }
}
