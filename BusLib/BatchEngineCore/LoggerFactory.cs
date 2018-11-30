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

        public static ILogger GetGroupLogger(int groupId, int groupKey)
        {
            throw new NotImplementedException();
        }

        public static ILogger GetProcessLogger(int processId, int processKey)
        {
            throw new NotImplementedException();
        }
    }
}