using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Serializers;

namespace BusLib.BatchEngineCore.Process
{
    internal interface IProcessRepository
    {
        IReadOnlyCollection<IBaseProcess> GetRegisteredProcesses();

        ITask GetProcessTaskHandler(int processKey);

        ISerializer GetSerializer(ITask taskHandler);
    }


    public interface IMasterSlaveObserver
    {
        void OnMaster();

        void OnSlave();
    }

    internal class ProcessRepository : IProcessRepository
    {
        readonly List<IBaseProcess> _registeredProcesses = new List<IBaseProcess>();
        private readonly ConcurrentDictionary<int, ITask> _taskExecutors = new ConcurrentDictionary<int, ITask>(); //todo scan from assemblies
        readonly ISerializersFactory _serializersFactory;
        private readonly ConcurrentDictionary<int, ISerializer> _taskSerializers = new ConcurrentDictionary<int, ISerializer>();
        readonly List<IMasterSlaveObserver> _masterSlaveObservers=new List<IMasterSlaveObserver>();

        public ProcessRepository()
        {
            _serializersFactory = SerializersFactory.Instance;
            //todo scan through ioc
            //todo verify process configurations on registration

            Scan();
        }

        public ITask GetProcessTaskHandler(int processKey)
        {
            if (_taskExecutors.TryGetValue(processKey, out ITask task))
                return task;

            return null;
        }

        public ISerializer GetSerializer(ITask taskHandler)
        {
            return _taskSerializers.GetOrAdd(taskHandler.ProcessKey, key =>
            {
                var serializer = taskHandler.Serializer;

                if (serializer == null)
                {
                    Type[] interfaces = taskHandler.GetType().GetInterfaces().Where(intrface => intrface.IsGenericType)
                        .ToArray();

                    var serializerType = interfaces.First(x => x.GetGenericTypeDefinition() == typeof(ITask<,>))
                        .GetGenericArguments().First(a => !typeof(ITaskContext).IsAssignableFrom(a));
                    serializer = _serializersFactory.GetSerializer(serializerType);
                }

                return serializer?? _serializersFactory.GetSerializer(typeof(object));
            });
        }

        void Scan()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic).ToList();
            var loadedPaths = loadedAssemblies.Select(a =>
            {
                try
                {
                    return a.Location;
                }
                catch (Exception)
                {
                    return null;
                }
            }).Where(a => !string.IsNullOrEmpty(a)).ToArray();

            var referencedPaths =
                Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"); //*.Definitions.dll
            var toLoad =
                referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase))
                    .ToList();
            toLoad.ForEach(
                path =>
                {
                    try
                    {
                        loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });


            var asmbs = AppDomain.CurrentDomain.GetAssemblies();
            var baseType = typeof(IBaseProcess);

            var types = asmbs.SelectMany(s =>
            {
                try
                {
                    return s.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(x => x != null);
                }
                catch (Exception)
                {
                    // ignored
                }

                return new Type[] { };

            }).Where(a =>
                a != null && a.IsPublic && a.IsAbstract == false && a.IsClass).ToList();

            var processes= types.Where(t=> baseType.IsAssignableFrom(t)).ToList();

            foreach (var process in processes)
            {
                var p = (IBaseProcess) Activator.CreateInstance(process);
                _registeredProcesses.Add(p);
            }

            var taskType = typeof(ITask);
            var tasks = types.Where(t => taskType.IsAssignableFrom(t)).ToList();

            foreach (var task in tasks)
            {
                ITask taskProcessor = (ITask) _registeredProcesses.FirstOrDefault(t=> t.GetType() ==task);
                if (taskProcessor == null)
                {
                    taskProcessor = (ITask)Activator.CreateInstance(task);
                }

                _taskExecutors.TryAdd(taskProcessor.ProcessKey, taskProcessor);
            }

            var obsType = typeof(IMasterSlaveObserver);
            var masterSlave = types.Where(t => obsType.IsAssignableFrom(t)).ToList();
            foreach (var type in masterSlave)
            {
                IMasterSlaveObserver ins = (IMasterSlaveObserver) Activator.CreateInstance(type);
                _masterSlaveObservers.Add(ins);
            }


        }

        public IReadOnlyCollection<IBaseProcess> GetRegisteredProcesses()
        {
            return _registeredProcesses.AsReadOnly();
        }

        internal void InvokeOnMaster()
        {
            foreach (var observer in _masterSlaveObservers)
            {
                Robustness.Instance.SafeCall(() =>
                {
                    observer.OnMaster();
                });
            }
        }

        internal void InvokeOnSlave()
        {
            foreach (var observer in _masterSlaveObservers)
            {
                Robustness.Instance.SafeCall(() =>
                {
                    observer.OnSlave();
                });
            }
        }
    }

}