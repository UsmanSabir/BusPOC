using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.JobSchedular;
using BusLib.Serializers;

namespace BusImpl
{
    internal class JobScheduler:IJobScheduler
    {
        private readonly IDistributedMessagePublisher _publisher;
        private readonly ISerializer _ser;
        protected internal readonly IFrameworkLogger SystemLogger;

        public JobScheduler(ISerializersFactory factory, IPubSubFactory pubSubFactory, IBatchLoggerFactory loggerFactory)
        {
            SystemLogger = loggerFactory.GetSystemLogger();
            _publisher = pubSubFactory.GetPublisher(CancellationToken.None, SystemLogger, nameof(IWatchDogMessage));
            //_ser = SerializersFactory.Instance.GetSerializer<List<JobCriteria>>();
            //JsonSerializer serializer=new JsonSerializer();
            _ser = factory.GetSerializer(typeof(JobCriteria));
        }

        public long CreateJob(int groupKey, List<JobCriteria> criteria, string submittedBy)
        {
            //Ibatchentity groupEntity = new BatchGroupStateWrapper(group)
            //{
            //    GroupKey = groupKey,
            //    Criteria = _ser.SerializeToString(criteria),
            //    IsGenerated = false,
            //    IsStopped = false,
            //    IsFinished = false,
            //    SubmittedBy = submittedBy,
            //    State = CompletionStatus.Pending.Name,
            //    IsManual = criteria.First().IsManual
            //};

            //create
            //Publish(groupEntity.Id);

            throw new NotImplementedException();
        }

        public long CreateJob(List<int> processKeys, List<JobCriteria> criteria, string submittedBy)
        {
            //BatchGroupState group = new BatchGroupState();
            //BatchGroupStateWrapper groupEntity = new BatchGroupStateWrapper(group)
            //{
            //    Payload = _ser.SerializeToString(processKeys),
            //    Criteria = _ser.SerializeToString(criteria),
            //    IsGenerated = false,
            //    IsStopped = false,
            //    IsFinished = false,
            //    SubmittedBy = submittedBy,
            //    State = CompletionStatus.Pending.Name,
            //    IsManual = criteria.First().IsManual
            //};

            //create
            //Publish(groupEntity.Id);

            throw new NotImplementedException();
        }

        void Publish(long groupId)
        {
            try
            {
                _publisher.PublishMessage(new ProcessGroupAddedMessage { GroupId = groupId }); 
                //_publisher.PublishMessage(new ProcessGroupAddedMessage { GroupId = groupId }); GroupMessage
            }
            catch (Exception e)
            {
                SystemLogger.Error("Failed to publish ProcessGroupAddedMessage for groupId {groupId} having error {error}", groupId, e);
            }
        }
    }
}