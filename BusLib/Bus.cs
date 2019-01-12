using BusLib.Core;
using BusLib.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.Handlers;
using BusLib.BatchEngineCore.Process;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.StatePersistence;
using BusLib.BatchEngineCore.Volume;
using BusLib.BatchEngineCore.WatchDog;
using BusLib.Core.Events;
using BusLib.Helper;
using BusLib.Infrastructure;
using BusLib.PipelineFilters;
using BusLib.ProcessLocks;
using BusLib.Serializers;

namespace BusLib
{
    public class Bus
    {
        //static Bus _instance;
        //public static Bus Instance => _instance ?? (_instance = new Bus());
        public IEventAggregator EventAggregator { get; private set; }
        
        private Pipeline<TaskMessage> _taskProcessorPipeline;

        //Pipeline<ICommand> _commandPipeLine;
        private readonly IFrameworkLogger _logger;
        private readonly TaskExecutorsPool _taskExecutorsRepo;
        //private Pipeline<GroupMessage> _grouPipeline;
        private Pipeline<IWatchDogMessage> _watchDogPipeline;
        //private readonly ProcessVolumePipeline _volumePipeline;
        private readonly StatePersistencePipeline _statePersistencePipeline;
        private readonly CacheStoragePipeline _cacheCommandPipeline;

        private IBatchEngineSubscribers _branchEngineSubscriber;

        //private CancellationTokenSource _cancellationTokenSource=;
        readonly CancellationToken _cancellationToken;
        private readonly ICacheAside _cacheAside;
        private readonly TaskProducerWorker _taskProducer;

        private readonly IEntityFactory EntityFactory;//todo
        private readonly IStateManager _stateManager;//todo
        private readonly IVolumeHandler VolumeHandler;//todo
        private readonly IProcessDataStorage _storage;//todo
        private readonly IPubSubFactory PubSubFactory;//todo
        private readonly IDistributedMutexFactory DistributedMutexFactory; //todo
        private readonly IResolver _resolver;
        private readonly IBatchLoggerFactory _batchLoggerFactory;

        private readonly IProcessRepository _processRepository;
        //private readonly ProcessWatchDog _watchDog;

        readonly ReaderWriterLockSlim _watchDogSync=new ReaderWriterLockSlim();
        private readonly DistributedMutex _leaderManager;
        private CancellationTokenSource _watchDogCancellationTokenSource= null;
        private DashboardService _dashboardService;
        private readonly CancellationTokenSource _cts;
        private readonly DatabasePipeline _databasePipeline;

        public Bus(IEntityFactory entityFactory, IVolumeHandler volumeHandler, IPubSubFactory pubSubFactory, IStateManager stateManager, IProcessDataStorage storage,
            IDistributedMutexFactory distributedMutexFactory, IResolver resolver, IBatchLoggerFactory batchLoggerFactory)
        {
            EntityFactory = entityFactory;
            VolumeHandler = volumeHandler;
            PubSubFactory = pubSubFactory;
            _stateManager = stateManager;
            _storage = storage;
            DistributedMutexFactory = distributedMutexFactory;
            _resolver = resolver;
            _batchLoggerFactory = batchLoggerFactory;
            _cts=new CancellationTokenSource();
            _cancellationToken = _cts.Token;
            HookExceptionEvents();

            _logger = batchLoggerFactory.GetSystemLogger();
            EventAggregator = new TinyEventAggregator();

            var wrapper = new BusStateManager(_stateManager, _logger, resolver);
            _stateManager = wrapper;

            var originalStorage = _storage;
            _storage = new CacheBusWrapper(_logger, originalStorage, resolver);
            _cacheCommandPipeline=new CacheStoragePipeline(_logger, _cancellationToken, originalStorage);
            

            _cacheAside = new CacheAside(_stateManager, _storage, EventAggregator, _logger, batchLoggerFactory);

            _processRepository = _resolver.Resolve<IProcessRepository>();
            //var taskListener = resolver.Resolve<ITaskListener>();
            //if (!ReferenceEquals(taskListener, _processRepository))
            //{
            //    Console.WriteLine("ALERT");
            //}

            _taskExecutorsRepo = new TaskExecutorsPool(_logger, _cacheAside, _cancellationToken, _stateManager, _processRepository, EventAggregator, resolver, _logger);
            
            
            //BuildCommandHandlerPipeline();
            _statePersistencePipeline = new StatePersistencePipeline(_logger, _cancellationToken);
            this._databasePipeline = new DatabasePipeline(_logger, _cancellationToken, 0);//todo 500

            _taskProcessorPipeline = GetTaskProcessorPipeLine();
            //_grouPipeline=new GroupHandlerPipeline(_stateManager, _logger, _branchEngineSubscriber);
            
            //_volumePipeline = new ProcessVolumePipeline(_cancellationToken, _logger, _stateManager, _cacheAside, _processRepository, VolumeHandler, resolver, EventAggregator, _branchEngineSubscriber);
            _branchEngineSubscriber = new BatchEngineSubscribers();
            
            //_watchDog = new ProcessWatchDog(_logger, StateManager, _branchEngineSubscriber, _cacheAside, SerializersFactory.Instance, EntityFactory, EventAggregator, Storage);
            //watchDog.Start(_cancellationToken);//todo
            // _grouPipeline = new Pipeline<GroupMessage>(_watchDog);
            //_watchDogPipeline = new Pipeline<IWatchDogMessage>(_watchDog);

            _taskProducer =new TaskProducerWorker(_logger, _cacheAside, VolumeHandler, resolver, batchLoggerFactory);

            _leaderManager = DistributedMutexFactory.CreateDistributedMutex(NodeSettings.Instance.LockKey, RunLocalWatchDog, () => SwitchToPubSubWatchDog(null), batchLoggerFactory.GetSystemLogger());
            
        }

        #region Master/Slave

        private Task RunLocalWatchDog(CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<bool>();
            token.Register(() =>
            {
                if (!_cancellationToken.IsCancellationRequested)
                    SwitchToPubSubWatchDog(completionSource);
            });

            SwitchToLocalWatchDog();
            
            return completionSource.Task;
        }

        private void SwitchToLocalWatchDog()
        {
            _watchDogSync.EnterWriteLock();
            try
            {
                _logger.Info("Switching to Master node");
                _watchDogCancellationTokenSource?.Cancel();

                if (_cancellationToken.IsCancellationRequested)
                {
                    _logger.Info("Bus stopped, can't switch to slave node");
                    return;
                }

                _watchDogCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);

                var volumePipeline = new ProcessVolumePipeline(_watchDogCancellationTokenSource.Token, _logger, _stateManager, _cacheAside, _processRepository, VolumeHandler, _resolver, EventAggregator, _branchEngineSubscriber);//todo move to watchdog class
                var watchDog = new ProcessWatchDog(_logger, _stateManager, _branchEngineSubscriber, _cacheAside, SerializersFactory.Instance, EntityFactory, 
                    EventAggregator, _storage, PubSubFactory, _resolver, _batchLoggerFactory, _processRepository, volumePipeline);
                _watchDogPipeline = new Pipeline<IWatchDogMessage>(watchDog);

                _watchDogCancellationTokenSource.Token.Register(() =>
                {
                    _logger.Info("Master node ended");
                    _logger.Debug("Master node ended");
                    watchDog.Dispose();
                });

                watchDog.Start(_watchDogCancellationTokenSource.Token);


                _processRepository.InvokeOnMaster();

                _leaderManager.InitializationComplete();
                _logger.Info("Switching to Master node complete");
                _logger.Debug("Running Master node");
            }
            catch (Exception e)
            {
                _logger.Error("Error switching watchdog to primary node", e);
            }
            finally
            {
                _watchDogSync.ExitWriteLock();
            }
        }

        private void SwitchToPubSubWatchDog(TaskCompletionSource<bool> completionSource)
        {
            _watchDogSync.EnterWriteLock();

            try
            {
                _logger.Info("Switching to Slave node");
                _watchDogCancellationTokenSource?.Cancel();
                if (_cancellationToken.IsCancellationRequested)
                {
                    _logger.Info("Bus stopped, can't switch to slave node");
                    return;
                }

                _watchDogCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);

                var pubSubWatchDog = new PubSubWatchDog(_logger, _stateManager, PubSubFactory, _cancellationToken, EventAggregator);
                _watchDogPipeline = new Pipeline<IWatchDogMessage>(pubSubWatchDog);

                _watchDogCancellationTokenSource.Token.Register(() =>
                {
                    _logger.Info("Slave node ended");
                    _logger.Debug("Slave node ended");
                    pubSubWatchDog.Dispose();
                });

                completionSource?.SetResult(true);
                _logger.Info("Switching to Slave node complete");
                _logger.Debug("Running Slave node");
                _processRepository.InvokeOnSlave();

                _leaderManager.InitializationComplete();
            }
            catch (Exception e)
            {
                _logger.Error("Error switching watchdog to secondary node", e);
                completionSource?.SetException(e);
            }
            finally
            {
                _watchDogSync.ExitWriteLock();
            }
        }

        #endregion

        #region Unhandled Exceptions

        private void HookExceptionEvents()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Fatal($"Unhandled task exception message {e.Exception?.GetBaseException()?.ToString() ?? string.Empty}", e.Exception);
            e.SetObserved();
            ((AggregateException)e.Exception).Handle(ex =>
            {
                _logger.Fatal($"Task unhandled exception type: {ex.ToString()}", ex);
                return true;
            });
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal($"Unhandled application error with terminating flag {e.IsTerminating} and message {e.ExceptionObject??string.Empty}");
            
        }

        #endregion

        bool ValidateEnvironment()
        {
            //todo
            return true;
        }

        public async Task Start()
        {
            if (!ValidateEnvironment())
            {
                return;
            }

            _dashboardService=new DashboardService(PubSubFactory, _logger, _storage, _stateManager, this);

            await _leaderManager.RunTaskWhenMutexAcquired(_cancellationToken).ContinueWith(r =>
            {
                _logger.Trace("LeaderSelector complete");
            }); // .Wait(_cancellationToken);
           
            _taskExecutorsRepo.Start(_cancellationToken);
            _taskProducer.Start(_cancellationToken);
            //_watchDog.Start(_cancellationToken);//todo. run on leader node only
            _dashboardService.Start(_cancellationToken);
        }

        public void Stop()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        //internal void HandleVolumeRequest(ProcessExecutionContext msg)
        //{
        //    _volumePipeline.Invoke(msg);
        //}


        //internal void HandleGroupMessage(GroupMessage msg)
        //{
        //    _grouPipeline.Invoke(msg);
        //}

        internal void HandleWatchDogMessage(IWatchDogMessage msg)
        {
            try
            {
                _watchDogSync.EnterReadLock();

                _watchDogPipeline.Invoke(msg);

            }
            finally
            {
                _watchDogSync.ExitReadLock();
            }
        }

        internal void HandleTaskMessage(TaskMessage msg)
        {
            _taskProcessorPipeline.Invoke(msg);
        }

        private Pipeline<TaskMessage> GetTaskProcessorPipeLine()
        {
            Pipeline<TaskMessage> tasksPipeline=new TaskProcessingPipeline(_batchLoggerFactory.GetSystemLogger(), _taskExecutorsRepo);
            return tasksPipeline;
        }

        //private void BuildCommandHandlerPipeline()
        //{
        //    _commandPipeLine = new Pipeline<ICommand>(new CommandHandler());
        //    _commandPipeLine.RegisterFeatureDecorator(new PerfMonitorHandler<ICommand>());
        //}
                
        //public void Execute(ICommand message)
        //{
        //    _commandPipeLine.Invoke(message);
        //}

        public void HandleDbCommand(Infrastructure.DbAction message)
        {
            _databasePipeline.Invoke(message);
        }


        internal void HandleStateManagerCommand(ActionCommand command)
        {
            _statePersistencePipeline.Invoke(command);
        }

        internal void HandleCacheStorageCommand(ActionCommand command)
        {
            _cacheCommandPipeline.Invoke(command);
        }


        internal void QueryAction<T>(Func<T> action, Action<T> onResult)
        {
            
        }

        public void ExecuteSystemCommand(ISystemCommand message)
        {
            if(message.PipeLineKey == nameof(ICommand))
            {
                //_commandPipeLine.HandleSystemCommand(message);
            }
        }


        //public void TestDecorator(ICommand command)
        //{
        //    Execute(command);

        //    //_decorator.Disable();

        //    Execute(command);

        //    //_decorator.Enable();
        //    Execute(command);
        //    Execute(command);

        //}
    }
}
