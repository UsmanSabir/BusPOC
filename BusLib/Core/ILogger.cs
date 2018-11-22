using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface ILogger
    {
        void Trace(string message);

        void Info(string info);

        void Warn(string warn);
        void Warn(string message, Exception e);
        void Error(string error);
        void Error(string error, Exception exception);
    }

    internal interface IFrameworkLogger : ILogger
    {
    }
}
