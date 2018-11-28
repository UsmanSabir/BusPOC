using BusLib.Core;
using BusLib.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BusLib.BatchEngineCore.Handlers
{
    internal class ApplicationTasksHandlers
    {
        static Lazy<ApplicationTasksHandlers> taskHandlers=new Lazy<ApplicationTasksHandlers>();
        public static ApplicationTasksHandlers Instance { get; } = taskHandlers.Value;

        ConcurrentDictionary<int, ITask> _taskExecutors = new ConcurrentDictionary<int, ITask>(); //todo scan from assemblies

        ISerializersFactory _serializersFactory;

        public ApplicationTasksHandlers(ISerializersFactory serializersFactory)
        {
            _serializersFactory = serializersFactory;
        }

        public ITask GetProcessTaskHandler(int processKey)
        {
            if (_taskExecutors.TryGetValue(processKey, out ITask task))
                return task;

            return null;
        }

        public ISerializer GetSerializer(ITask taskHandler)
        {
            var serializer = taskHandler.Serializer;

            if (serializer == null)
            {
                Type[] interfaces = taskHandler.GetType().GetInterfaces().Where(intrface => intrface.IsGenericType).ToArray();

                var serializerType = interfaces.First(x => x.GetGenericTypeDefinition() == typeof(ITask<,>)).GetGenericArguments().First(a=> !typeof(ITaskContext).IsAssignableFrom(a));
                serializer = _serializersFactory.GetSerializer(serializerType);
            }
            return serializer;
        }
    }
}