using System;
using BusImpl.Logs;
using NLog;
using ILogger = BusLib.Core.ILogger;

namespace BusImpl.SeriLog
{
    public class NLogWrapper:ILogger
    {
        private readonly Logger _loggerImplementation;

        public NLogWrapper(string name=null)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "logger";

            _loggerImplementation = NLogFactory.Create(name);
        }


        public void Debug(string msg)
        {
            Console.WriteLine(msg);
        }

        public void Trace(string message, params object[] args)
        {
            _loggerImplementation.Trace(message, args);
        }

        public void Info(string info, params object[] args)
        {
            _loggerImplementation.Info(info, args);
        }

        public void Warn(string warn, params object[] args)
        {
            _loggerImplementation.Warn(warn, args);
        }

        public void Warn(string message, Exception e, params object[] args)
        {
            _loggerImplementation.Warn(message, e, args);
        }

        public void Error(string error)
        {
            _loggerImplementation.Error(error);
        }

        public void Error(string error, params object[] args)
        {
            _loggerImplementation.Error(error, args);
        }

        public void Error(string error, Exception exception, params object[] args)
        {
            _loggerImplementation.Error(error, exception, args);
        }

        public void Fatal(string error, params object[] args)
        {
            _loggerImplementation.Fatal(error, args);
        }

        public void Fatal(string error, Exception exception, params object[] args)
        {
            _loggerImplementation.Fatal(error, exception, args);
        }
    }
}