using System;
using BusLib.Core;

namespace BusLib.BatchEngineCore
{
    //todo
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string name = null);
    }

    public interface IBatchLoggerFactory
    {
        IFrameworkLogger GetSystemLogger();
        ILogger GetTaskLogger(long taskId, long processId, Guid correlationId);
        ILogger GetGroupLogger(long groupId, int groupKey);
        ILogger GetProcessLogger(long processId, long processKey, Guid correlationId);
        ILogger CreateLogger(string name = null);
    }

    public class BatchLoggerFactory : IBatchLoggerFactory
    {
        public BatchLoggerFactory(ILoggerFactory instance)
        {
            Instance = instance;
        }

        ILoggerFactory Instance { get; set; } 

        public IFrameworkLogger GetSystemLogger()
        {
            return new FrameworkLogger(CreateLogger("System"));
        }

        public ILogger GetTaskLogger(long taskId, long processId, Guid correlationId)
        {
            return new PrependedLogger(CreateLogger(), $"TaskId {taskId}, ProcessId {processId}, correlationId {correlationId} =>");
        }

        public ILogger GetGroupLogger(long groupId, int groupKey)
        {
            return new PrependedLogger(CreateLogger(), $"GroupId {groupId}, GroupKey {groupKey} =>");
        }

        public ILogger GetProcessLogger(long processId, long processKey, Guid correlationId)
        {
            return new PrependedLogger(CreateLogger(), $"ProcessId {processId}, ProcessKey {processKey}, correlationId {correlationId}  =>");
        }

        public ILogger CreateLogger(string name = null)
        {
            return Instance.CreateLogger(name);
        }
    }

    class FrameworkLogger:IFrameworkLogger
    {
        private readonly ILogger _frameworkLoggerImplementation;

        public FrameworkLogger(ILogger frameworkLoggerImplementation)
        {
            _frameworkLoggerImplementation = frameworkLoggerImplementation;
        }

        public void Trace(string message, params object[] args)
        {
            _frameworkLoggerImplementation.Trace(message, args);
        }

        public void Info(string info, params object[] args)
        {
            _frameworkLoggerImplementation.Info(info, args);
        }

        public void Warn(string warn, params object[] args)
        {
            _frameworkLoggerImplementation.Warn(warn, args);
        }

        public void Warn(string message, Exception e, params object[] args)
        {
            _frameworkLoggerImplementation.Warn(message, e, args);
        }

        public void Error(string error)
        {
            _frameworkLoggerImplementation.Error(error);
        }

        public void Error(string error, params object[] args)
        {
            _frameworkLoggerImplementation.Error(error, args);
        }

        public void Error(string error, Exception exception, params object[] args)
        {
            _frameworkLoggerImplementation.Error(error, exception, args);
        }

        public void Fatal(string error, params object[] args)
        {
            _frameworkLoggerImplementation.Fatal(error, args);
        }

        public void Fatal(string error, Exception exception, params object[] args)
        {
            _frameworkLoggerImplementation.Fatal(error, exception, args);
        }
    }

    class ConsoleLogger:ILogger, IFrameworkLogger
    {
        public void Trace(string message)
        {
            Console.WriteLine(message);
        }

        public void Info(string info, object te)
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

        public void Trace(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(string info, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(string warn, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(string message, Exception e, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(string error)
        {
            Console.WriteLine(error);
        }

        public void Error(string error, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(string error, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string error, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string error, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
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