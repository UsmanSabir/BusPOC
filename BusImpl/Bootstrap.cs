using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using BusImpl.Db;
using BusImpl.Logs;
using BusImpl.PubSubImpl.Udp;
using BusImpl.Redis;
using BusImpl.SeriLog;
using BusImpl.Sql;
using BusLib;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Process;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Infrastructure;
using BusLib.JobSchedular;
using BusLib.JobScheduler;
using BusLib.ProcessLocks;
using BusLib.Serializers;

namespace BusImpl
{
    public class Bootstrap
    {
        //for unity https://stackoverflow.com/a/6110813
        public static IResolver Initialize(string redisConnection = "10.39.100.27")
        {
            SerializersFactory.Instance.DefaultSerializer = new JsonSerializer();

            Autofac.ContainerBuilder builder = new ContainerBuilder();

            //builder.RegisterType<SerializersFactory>()
            //    .As<ISerializersFactory>()
            //    .SingleInstance();

            builder.RegisterType<Bus>()
                .InstancePerLifetimeScope();

            builder.RegisterType<EntityFactory>()
                .As<IEntityFactory>();

            builder.RegisterType<StatePersistenceService>()
                .As<IStateManager>();

            builder.RegisterType<VolumeHandler>()
                .As<IVolumeHandler>();

            builder.RegisterType<JsonSerializer>()
                .As<ISerializer>();

            builder.Register(r=> SerializersFactory.Instance)
                .As<ISerializersFactory>();
            
            builder.Register<IProcessDataStorage>(r =>
            {
                var serializer = r.Resolve<ISerializer>();
                return new RedisProcessStorage(NodeSettings.Instance.Name, redisConnection, serializer);
            });

            builder.Register<IPubSubFactory>(r => { return new RedisPubSubFactory(redisConnection); });

            //builder.Register<IPubSubFactory>(r => { return new UdpPubSubFactory("239.0.0.222", 2121, r.Resolve<IBatchLoggerFactory>()); }); //2222

            //builder.Register<IDistributedMutexFactory>(r => new DistributedMutexFactory(redisConnection));
            builder.Register<IDistributedMutexFactory>(r => new SqlLockFactory());
            



            builder.RegisterType<BatchEngineService>();

            builder.RegisterType<NLogFactory>()
                .As<ILoggerFactory>()
                .SingleInstance();

            builder.RegisterType<JobSchedulerInternal>()
                .As<IJobScheduler>()
                .SingleInstance();
            

            builder.RegisterType<BatchLoggerFactory>()
                .As<IBatchLoggerFactory>()
                .SingleInstance();

            builder.RegisterType<JobScheduler>()
                .As<IJobScheduler>()
                .SingleInstance();

            //var processRepository = new ProcessRepository();
            builder.RegisterType<ProcessRepository>()
                .AsSelf()
                .As<IProcessRepository>()
                .As<ITaskListenerHandler>()
                .SingleInstance();


            var taskListenerType = typeof(ITaskListener);
            foreach (var assembly in GetAssemblies())
            {
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => taskListenerType.IsAssignableFrom(t))
                    .Except<ProcessRepository>()
                    //.InstancePerLifetimeScope()
                    .SingleInstance()
                    .AsImplementedInterfaces()
                    ;
            }

            
             //builder.RegisterAssemblyTypes(typeof(Bootstrap).Assembly)
             //   .Where(t => taskListenerType.IsAssignableFrom(t))
             //   .InstancePerLifetimeScope()
             //   .AsImplementedInterfaces();

            //builder.RegisterDecorator<IBatchLoggerFactory>(
            //    (c, inner) => new IBatchLoggerFactory(inner),
            //    fromKey: "handler");


            //todo
            //var redisConnection = "10.39.100.27";// "localhost";// "172.31.15.19";
            //var redisConnection = "172.31.15.19";

            //var serializer = SerializersFactory.Instance.DefaultSerializer;

            //IProcessDataStorage storage = new RedisProcessStorage(NodeSettings.Instance.Name, redisConnection, serializer);

            //IPubSubFactory factory = new RedisPubSubFactory(redisConnection);
            //IDistributedMutexFactory mutexFactory = new DistributedMutexFactory(redisConnection);

            //Bus.EntityFactory = new EntityFactory();
            //Bus.StateManager = new StatePersistenceService();
            //Bus.VolumeHandler = new VolumeHandler();
            //Bus.Storage = storage;
            //Bus.PubSubFactory = factory;
            //Bus.DistributedMutexFactory = mutexFactory;

            //builder.Register<ContainerResolver>(c => { return new ContainerResolver(c.); })
            //    .As<IResolver>();

            IContainer container = null;
            builder.Register(c => container).AsSelf();
            builder.RegisterBuildCallback(c => container = c);

            builder.RegisterType<ContainerResolver>()
                .As<IResolver>();

            var container1 = builder.Build();
            //BatchLoggerFactory.Instance = container1.Resolve<ILoggerFactory>();
            //var bld = new ContainerResolver();
            //return bld;

            var resolver = container1.Resolve<IResolver>();
            Resolver.Instance = resolver;
            

            OrmWrapper.WrapOrmActions(resolver.Resolve<Bus>());


            return resolver;
        }

        //static void Do()
        //{
        //    var con = Initialize();
        //    var bus = con.Resolve<Bus>();
        //    bus.Start()
        //}


        static IEnumerable<Assembly> GetAssemblies()
        {
            //https://autofaccn.readthedocs.io/en/latest/register/scanning.html
            //BuildManager
            var asmbs = AppDomain.CurrentDomain.GetAssemblies();//todo referenced assemblies
            //var baseType = typeof(IBaseProcess);

            //var types = asmbs.SelectMany(s =>
            //{
            //    try
            //    {
            //        return s.GetTypes();
            //    }
            //    catch (ReflectionTypeLoadException ex)
            //    {
            //        return ex.Types.Where(x => x != null);
            //    }
            //    catch (Exception)
            //    {
            //        // ignored
            //    }

            //    return new Type[] { };

            //}).Where(a =>
            //    a != null && a.IsPublic && a.IsAbstract == false && a.IsClass).ToList();
            return asmbs;
        }
    }
}