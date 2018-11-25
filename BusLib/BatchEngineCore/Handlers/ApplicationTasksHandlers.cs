using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BusLib.BatchEngineCore.Handlers
{
    public class ApplicationTasksHandlers
    {
        static Lazy<ApplicationTasksHandlers> taskHandlers=new Lazy<ApplicationTasksHandlers>();
        public static ApplicationTasksHandlers Instance { get; } = taskHandlers.Value;

        ConcurrentDictionary<int, ITask> _taskExecutors = new ConcurrentDictionary<int, ITask>();

        public ITask GetProcessTaskHandler(int processKey)
        {
            if (_taskExecutors.TryGetValue(processKey, out ITask task))
                return task;

            return null;
        }
    }
}