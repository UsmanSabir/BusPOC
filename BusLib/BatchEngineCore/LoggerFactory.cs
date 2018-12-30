using System;
using BusLib.Core;

namespace BusLib.BatchEngineCore
{
    //todo
    public class LoggerFactory
    {
        internal static IFrameworkLogger GetSystemLogger()
        {
            return new ConsoleLogger();
        }

        public static ILogger GetTaskLogger(long taskId, long processId, Guid correlationId)
        {
            return new ConsoleLogger();
        }

        public static ILogger GetGroupLogger(long groupId, int groupKey)
        {
            return new ConsoleLogger();
        }

        public static ILogger GetProcessLogger(long processId, long processKey)
        {
            return new ConsoleLogger();
        }
    }

    class ConsoleLogger:ILogger, IFrameworkLogger
    {
        public void Trace(string message)
        {
            Console.WriteLine(message);
        }

        public void Info(string info)
        {
            Console.WriteLine(info);
        }

        public void Warn(string warn)
        {
            Console.WriteLine(warn);
        }

        public void Warn(string message, Exception e)
        {
            Console.WriteLine(message + e.ToString());
        }

        public void Error(string error)
        {
            Console.WriteLine(error);
        }

        public void Error(string error, Exception exception)
        {
            Console.WriteLine(error + exception);
        }

        public void Fetal(string error)
        {
            Console.WriteLine(error);
        }

        public void Fetal(string error, Exception exception)
        {
            Console.WriteLine(error + exception);
        }
    }
}