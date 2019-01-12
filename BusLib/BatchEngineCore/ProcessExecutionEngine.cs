using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.BatchEngineCore.Process;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Core.Events;
using BusLib.Helper;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore
{
    //pipeline
    class ProcessVolumeRequestHandler: IHandler<ProcessExecutionContext>
    {
        private readonly IProcessRepository _registeredProcesses;
        private readonly ICacheAside _cacheAside;
        readonly ManualResetEvent _pauseHandler=new ManualResetEvent(true);
        private readonly IFrameworkLogger _logger;
        private readonly IStateManager _stateManager;
        private readonly IBatchEngineSubscribers _batchEngineSubscribers;

        readonly ConcurrentDictionary<int, Pipeline<ProcessExecutionContext>> _processPipelines = new ConcurrentDictionary<int, Pipeline<ProcessExecutionContext>>();
        private int _delayInRetries = 3000; //todo
        private readonly IVolumeHandler _volumeHandler;
        private readonly CancellationToken _token;
        private readonly IResolver _resolver;
        private readonly IEventAggregator _eventAggregator;

        public ProcessVolumeRequestHandler(IFrameworkLogger logger, IStateManager stateManager, ICacheAside cacheAside, 
            IProcessRepository registeredProcesses, IVolumeHandler volumeHandler, CancellationToken token, 
            IResolver resolver, IEventAggregator eventAggregator, IBatchEngineSubscribers batchEngineSubscribers)
        {
            _logger = logger;
            _stateManager = stateManager;
            _cacheAside = cacheAside;
            _registeredProcesses = registeredProcesses;
            _volumeHandler = volumeHandler;
            _token = token;
            _resolver = resolver;
            _eventAggregator = eventAggregator;
            _batchEngineSubscribers = batchEngineSubscribers;
        }

        //void InvokeProcessCompletion(ProcessExecutionContext context, bool isFailed)
        //{
        //    var process = _registeredProcesses.GetRegisteredProcesses().FirstOrDefault(p => p.ProcessKey == context.ProcessState.ProcessKey);

        //    process?.InvokeProcessCompeteEvent(context, isFailed);

        //    var subscribers = _batchEngineSubscribers.GetProcessSubscribers().Where(p=>p.ProcessKey==context.ProcessState.ProcessKey);
        //    //ProcessRetryContext ct =new ProcessRetryContext(context.ProcessState.Id, context.ProcessState.ProcessKey, context.Logger);
        //    ProcessCompleteContext ct = new ProcessCompleteContext(context.ProcessState.Id, context.ProcessState.ProcessKey, false, false, context.Logger);
            
        //    //IProcessCompleteContext ctx=new proccom
        //    foreach (var subscriber in subscribers)
        //    {
        //        subscriber.OnProcessComplete(ct);
        //    }
        //}

        void Execute(ProcessExecutionContext context)
        {
            _logger.Trace(context.GetFormattedLogMessage("Volume request received"));
            _pauseHandler.WaitOne();

            var processKey = context.Configuration.ProcessKey; 
            var processId = context.ProcessState.ProcessId;
            Pipeline<ProcessExecutionContext> pipeline;
            var process = _registeredProcesses.GetRegisteredProcesses().FirstOrDefault(p => p.ProcessKey == processKey);

            try
            {
                pipeline = GetPipeline(processId);
            }
            catch (Exception e)//todo what to do in-case of FrameworkException
            {
                if (_token.IsCancellationRequested)
                {
                    _logger.Warn("Volume generation cancelled");
                    return;
                }
                pipeline = null;
                var error = context.GetFormattedLogMessage($"Failed to build process pipeline with message {e.Message}", e);
                _logger.Error(error, e);

                HandleError(context, error);
                return;
            }

            if (pipeline == null)
            {
                if (_token.IsCancellationRequested)
                {
                    _logger.Warn("Volume generation cancelled");
                    return;
                }

                var error = context.GetFormattedLogMessage("Volume handler not found");
                _logger.Error(error);

                HandleError(context, error);
                //context.MarkAsError(_stateManager, error, _registeredProcesses, _batchEngineSubscribers, _logger);
                ////InvokeProcessCompletion(context, true);
                //_eventAggregator.PublishAsync(this, Constants.EventProcessFinished, context.ProcessState.Id.ToString());//todo check do we need it
                //_eventAggregator.PublishAsync(this, Constants.EventCheckGroupCommand, context.ProcessState.GroupId.ToString());
                return;
            }

            _logger.Trace(context.GetFormattedLogMessage("Volume request sending to pipeline"));

            try
            {
                pipeline.Invoke(context);
            }
            catch (Exception e) //todo what to do in-case of FrameworkException
            {
                if (_token.IsCancellationRequested)
                {
                    _logger.Warn("Volume generation cancelled");
                    return;
                }
                var error = context.GetFormattedLogMessage("Error generating volume", e);
                _logger.Error(error);
                HandleError(context, error);
                //context.MarkAsError(_stateManager, error, _registeredProcesses, _batchEngineSubscribers, _logger);
                
                //InvokeProcessCompletion(context, true);
                //_eventAggregator.PublishAsync(this, Constants.EventProcessFinished, context.ProcessState.Id.ToString());//todo check do we need it
                //_eventAggregator.PublishAsync(this, Constants.EventCheckGroupCommand, context.ProcessState.GroupId.ToString());
            }

            //var t = typeof(GenericProcessHandler<>);
            //var gType = t.MakeGenericType(processInstance.VolumeDataType);
            //var handler = Activator.CreateInstance(gType, processInstance, context);

            ////update start date

            //if( processInstance.VolumeDataType == typeof(int))
            //{
            //    var pr = GenericProcessHandler.Create(processInstance);

            //    //send to query pipeline
            //    IBaseProcess<int> p = (IBaseProcess<int>)processInstance;
            //    Bus.Instance.QueryAction(()=> p.GetVolume(context), HandleVolume) //IEnumerable<int>
            //    ;
            //}
            //else
            //{
            //    //todo
            //}

        }

        private void HandleError(ProcessExecutionContext context, string error)
        {
            context.MarkAsError(_stateManager, error, _registeredProcesses, _batchEngineSubscribers, _logger);
            if (context.Configuration.ErrorThreshold.HasValue && context.Configuration.ErrorThreshold.Value > 0)
            {
                _eventAggregator.Publish<VolumeErrorMessage>(new VolumeErrorMessage(this, context)); //check and stop group
            }

            if (context.Configuration.ErrorThreshold.HasValue && context.Configuration.ErrorThreshold.Value > 0)
            {
                _eventAggregator.Publish<VolumeErrorMessage>(new VolumeErrorMessage(this, context));//check and stop group
            }

            _eventAggregator.PublishAsync(this, Constants.EventProcessFinished, context.ProcessState.Id.ToString()); //todo check do we need it
            _eventAggregator.PublishAsync(this, Constants.EventCheckGroupCommand, context.ProcessState.GroupId.ToString());
        }

        Pipeline<ProcessExecutionContext> GetPipeline(int processKey)
        {
            var pipeLine = _processPipelines.GetOrAdd(processKey, key =>
            {
                var processConfig = _cacheAside.GetProcessConfiguration(key);
                
                var process = _registeredProcesses.GetRegisteredProcesses().FirstOrDefault(p => p.ProcessKey == processConfig.ProcessKey); //processKey
                if (process == null)
                    return null;
                var maxVolumeRetries = processConfig.MaxVolumeRetries;
                //if (maxVolumeRetries < -1)
                //{
                    
                //}

                Pipeline<ProcessExecutionContext> pipeline = new Pipeline<ProcessExecutionContext>(
                    new VolumeGenerator(process, _stateManager, _volumeHandler, _resolver, _token, _registeredProcesses, _batchEngineSubscribers, _logger));
                
                if (maxVolumeRetries != 0) // -1 means unlimited retries
                {
                    pipeline.RegisterFeatureDecorator(
                        new RetryFeatureHandler<ProcessExecutionContext>(maxVolumeRetries,
                            _delayInRetries, _logger, _token));
                }

                //var duplicateCheckFilter = new DuplicateCheckFilter<IProcessExecutionContext,int>( (context => context.ProcessState.Id), $"VolumeGenerator{processKey}", _logger);
                //pipeline.RegisterFeatureDecorator(duplicateCheckFilter);
                //var refr=new WeakReference<DuplicateCheckFilter<IProcessExecutionContext, int>>(duplicateCheckFilter); //todo
                //Bus.Instance.EventAggregator.Subscribe<ProcessGroupRemovedMessage>(msg =>
                //{
                //    if (refr.TryGetTarget(out DuplicateCheckFilter<IProcessExecutionContext, int> filt))
                //    {
                //        filt.Cleanup(msg.Group.ProcessEntities.Select(r => r.Id));
                //    }
                //});
                pipeline.RegisterFeatureDecorator(new ThrottlingFilter<ProcessExecutionContext>(1, _logger));

                return pipeline;
            });
            return pipeLine;
        }

        public void Handle(ProcessExecutionContext message)
        {
            Execute(message);
        }

        class VolumeGenerator:IHandler<ProcessExecutionContext>
        {
            private readonly IBaseProcess _process;
            private readonly IStateManager _stateManager;
            private readonly IVolumeHandler _volumeHandler; //todo initialize based on configuration or fixed?
            private readonly IResolver _resolver;
            //private readonly Action<ProcessExecutionContext, bool> _invokeProcessCompletion;


            public VolumeGenerator(IBaseProcess process, IStateManager stateManager, IVolumeHandler volumeHandler,
                IResolver resolver, CancellationToken cancellationToken, IProcessRepository registeredProcesses,
                IBatchEngineSubscribers batchEngineSubscribers, IFrameworkLogger logger)
            {
                _process = process;
                _stateManager = stateManager;
                this._volumeHandler = volumeHandler;
                _resolver = resolver;
                _cancellationToken = cancellationToken;
                _registeredProcesses = registeredProcesses;
                _batchEngineSubscribers = batchEngineSubscribers;
                _logger = logger;
            }

            private Bus _bus;
            private readonly CancellationToken _cancellationToken;
            private readonly IProcessRepository _registeredProcesses;
            private readonly IBatchEngineSubscribers _batchEngineSubscribers;
            private readonly IFrameworkLogger _logger;

            private Bus Bus
            {
                get { return _bus ?? (_bus = _resolver.Resolve<Bus>()); }
            }


            public void Handle(ProcessExecutionContext message)
            {
                message.Logger.Trace("Volume handler ");

                var processState = _stateManager.GetAndStartProcessById(message.ProcessState.Id);
                if (processState.CanGenerateProcessVolume())
                {
                    _logger.Warn("Cannot generate volume if process is not 'New' or process is Stopped PQId {PId}", processState.Id);
                    return;
                }

                var canExecute = _process.CanExecute(message);
                if (!canExecute)
                {
                    message.Logger.Info("Pre-execute condition failed");
                    processState.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Success, "Pre-execute condition failed", _stateManager,
                        _registeredProcesses, message, _batchEngineSubscribers, _logger);
                    //_process.InvokeProcessCompeteEvent(message);
                    Bus.EventAggregator.PublishAsync(this, Constants.EventProcessFinished, processState.Id.ToString());//todo check do we need it
                    Bus.EventAggregator.PublishAsync(this, Constants.EventCheckGroupCommand, processState.GroupId.ToString());
                    return;
                }



                if (_process is IHaveSupportingData processWithSupportingData)
                {
                    message.Logger.Trace("Initializing Supporting Data ");

                    //initializer process wise cache data
                    Robustness.Instance.SafeCallWithRetry(() => processWithSupportingData.InitializeSupportingData(message), 3, 1000, message.Logger);
                }

                _process.HandleVolume(_volumeHandler, message, _cancellationToken);
                message.Logger.Info("Volume generated return successfully");
                if (!message.HasVolume)
                {
                    message.Logger.Info("No volume returned");
                    processState.IsVolumeGenerated = true;//todo: volume generated for no volume???
                    processState.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Success, "No volume", _stateManager,
                        _registeredProcesses, message, _batchEngineSubscribers, _logger);
                    //_process.InvokeProcessCompeteEvent(message);
                    //_invokeProcessCompletion(message, false); //moved to extensions
                    Bus.EventAggregator.PublishAsync(this, Constants.EventProcessFinished, processState.Id.ToString());//todo check do we need it
                    Bus.EventAggregator.PublishAsync(this, Constants.EventCheckGroupCommand, processState.GroupId.ToString());
                    return;
                }
                
                //message.
                
                message.Logger.Trace("Marking process as Generated");
                processState.HasVolume = message.HasVolume;
                processState.MarkAsVolumeGenerated(_stateManager);
                Bus.EventAggregator.PublishAsync(this, Constants.EventProcessVolumeGenerated, processState.Id.ToString());
            }

            public void Dispose()
            {
            }
        }

        //private void HandleVolume(IEnumerable<int> obj)
        //{
        //    throw new NotImplementedException();
        //}

        //class GenericProcessHandler
        //{
        //    internal static GenericProcessHandler<T> Create<T>(T it)
        //    {
        //        return new GenericProcessHandler<T>();
        //    }
        //}

        //class GenericProcessHandler<T>
        //{
        //    IBaseProcess<T> _process;
        //    IProcessExecutionContext _context;
        //    IBaseProcess<T> GetProcess(string key)
        //    {
        //        IEnumerable<T> res = _process.GetVolume(_context);
        //        //todo persist
        //        throw new NotImplementedException();
        //    }
        //}

        public void Dispose()
        {
            _pauseHandler?.Dispose();
        }
    }

}
