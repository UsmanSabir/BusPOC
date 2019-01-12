using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Core.Events;
using BusLib.Helper;
using BusLib.JobSchedular;
using BusLib.Serializers;

namespace BusLib.JobScheduler
{
    public class JobSchedulerInternal :IJobScheduler
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly IStateManager _stateManager;
        private readonly IEntityFactory _entityFactory;
        private readonly ISerializer _ser;

        public JobSchedulerInternal(IStateManager stateManager, IEntityFactory entityFactory, ILogger logger, IEventAggregator eventAggregator) //IDistributedMessagePublisher watchDogPublisher, 
        {
            _stateManager = stateManager;
            _logger = logger;
            _eventAggregator = eventAggregator;

            _entityFactory = entityFactory;
            _ser = SerializersFactory.Instance.GetSerializer<List<JobCriteria>>();
        }

        public long CreateJob(int groupKey, List<JobCriteria> criteria, string submittedBy)
        {
            _logger.Trace($"Create group request received for groupKey {groupKey} with criteria {criteria}");

            var groupEntity = _entityFactory.CreateGroupEntity();
            groupEntity.GroupKey = groupKey;
            groupEntity.Criteria = _ser.SerializeToString(criteria);
            groupEntity.IsGenerated = false;
            groupEntity.IsStopped = false;
            groupEntity.IsFinished = false;
            groupEntity.SubmittedBy = submittedBy;
            groupEntity.State = CompletionStatus.Pending.Name;
            groupEntity.IsManual = criteria.First().IsManual;

            var entity = _stateManager.CreateGroupEntity(groupEntity);
            
            //Robustness.Instance.SafeCall(()=> _watchDogPublisher.PublishMessage(new ProcessGroupAddedMessage{ GroupId = entity.Id}));

            _eventAggregator.PublishAsync(this, Constants.EventProcessGroupAdded, entity.Id.ToString());

            _logger.Trace($"Create group request complete for groupKey {groupKey} with criteria {criteria}");

            return entity.Id;
        }

        public long CreateJob(List<int> processKeys, List<JobCriteria> criteria, string submittedBy)
        {
            _logger.Trace($"Create process request received for process {processKeys} with criteria {criteria}");

            var groupEntity = _entityFactory.CreateGroupEntity();
            groupEntity.Payload = _ser.SerializeToString(processKeys);
            groupEntity.Criteria = _ser.SerializeToString(criteria);
            groupEntity.IsGenerated = false;
            groupEntity.IsStopped = false;
            groupEntity.SubmittedBy = submittedBy;
            groupEntity.IsFinished = false;
            groupEntity.State = CompletionStatus.Pending.Name;
            groupEntity.IsManual = criteria.First().IsManual;

            var entity = _stateManager.CreateGroupEntity(groupEntity);

            _eventAggregator.PublishAsync(this, Constants.EventProcessGroupAdded, entity.Id.ToString());

            //Robustness.Instance.SafeCall(() =>
            //    _watchDogPublisher.PublishMessage(new ProcessGroupAddedMessage { GroupId = entity.Id })); //_watchDogPublisher.PublishMessage(new ProcessGroupAddedMessage { GroupId = entity.Id })

            _logger.Trace($"Create process request complete for process {processKeys} with criteria {criteria}");

            return entity.Id;
        }
    }
}