using BusLib.Core;
using BusLib.Core.Events;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Groups
{
    public class ProcessErrorWatcher:SafeDisposable
    {
        private readonly ICacheAside _cacheAside;
        private readonly IEventAggregator _eventAggregator;
        private readonly IStateManager _stateManager;
        private readonly TinyMessageSubscriptionToken _subFinish;
        private readonly TinyMessageSubscriptionToken[] _subscriptions;
        public ProcessErrorWatcher(ICacheAside cacheAside, IEventAggregator eventAggregator, IStateManager stateManager)
        {
            _cacheAside = cacheAside;
            _eventAggregator = eventAggregator;
            _stateManager = stateManager;

            _subFinish = _eventAggregator.Subscribe<TextMessage>(ProcessFinish, Constants.EventProcessFinished);

            _subscriptions= new TinyMessageSubscriptionToken[]
            {
                _subFinish
            };
        }

        private void ProcessFinish(TextMessage message)
        {
            if (int.TryParse(message.Parameter, out int processId))
            {
                var configuration = _cacheAside.GetProcessConfiguration(processId);
                if (configuration.ErrorThreshold.HasValue && configuration.ErrorThreshold.Value > 0)
                {
                    //_stateManager.CountFailedTasksForProcess<>()
                }
            }
            
            //_eventAggregator.PublishAsync(this, Constants.EventCheckGroupCommand, context.ProcessState.GroupId.ToString());
        }


        protected override void Dispose(bool disposing)
        {
            foreach (var token in _subscriptions)
            {
                _eventAggregator.Unsubscribe(token);
            }
            

            base.Dispose(disposing);
        }
    }
}