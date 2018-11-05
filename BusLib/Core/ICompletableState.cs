using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface ICompletableState
    {
        bool IsComplete { get; }
    }
}
