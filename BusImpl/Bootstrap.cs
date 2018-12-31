using Autofac;
using BusImpl.Db;
using BusImpl.Logs;
using BusImpl.Redis;
using BusImpl.SeriLog;
using BusLib;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Infrastructure;
using BusLib.ProcessLocks;
using BusLib.Serializers;

namespace BusImpl
{
    public class Bootstrap
    {
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

            builder.Register<IProcessDataStorage>(r =>
            {
                var serializer = r.Resolve<ISerializer>();
                return new RedisProcessStorage(NodeSettings.Instance.Name, redisConnection, serializer);
            });

            builder.Register<IPubSubFactory>(r => { return new RedisPubSubFactory(redisConnection); });

            builder.Register<IDistributedMutexFactory>(r => new DistributedMutexFactory(redisConnection));

            
            builder.RegisterType<BatchEngineService>();

            builder.RegisterType<NLogFactory>()
                .As<ILoggerFactory>()
                .SingleInstance();

            builder.RegisterType<BatchLoggerFactory>()
                .As<IBatchLoggerFactory>()
                .SingleInstance();


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
        
    }
}