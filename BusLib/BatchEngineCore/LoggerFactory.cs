using System;
using BusLib.Core;

namespace BusLib.BatchEngineCore
{
    public class LoggerFactory
    {
        public static ILogger GetSystemLogger()
        {
            throw new NotImplementedException();
        }

        public static ILogger GetTaskLogger(int taskId, int processId, Guid correlationId)
        {
            throw new NotImplementedException();
        }
    }
}