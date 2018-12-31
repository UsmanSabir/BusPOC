using System;
using System.Collections.Generic;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core;
using BusLib.Serializers;

namespace BusLib
{
    public class BatchEngineService
    {
        private readonly IStateManager _stateManager;
        private readonly ISerializersFactory _serializersFactory = SerializersFactory.Instance;
        private readonly IEntityFactory _entityFactory;
        private readonly Bus _bus;

        public BatchEngineService(IStateManager stateManager, IEntityFactory entityFactory, Bus bus)
        {
            _stateManager = stateManager;
            _entityFactory = entityFactory;
            _bus = bus;
        }

        public void Start()
        {
            _bus.Start().Wait();
        }
        public void Stop()
        {
            _bus.Stop();
        }


        public void SubmitSingleProcess<T>(JobCriteria criteria, bool isManual = true, string triggeredBy = "System") where T: IBaseProcess
        {
            var processKey = Activator.CreateInstance<T>().ProcessKey;
            SubmitSingleProcess(processKey, criteria, isManual);
        }

        public void SubmitGroup(int groupKey, List<JobCriteria> criteria, bool isManual, string triggeredBy)
        {
            var entity = CreateGroupEntity(groupKey, criteria, isManual, triggeredBy);

            _bus.HandleWatchDogMessage(new GroupMessage(GroupActions.Start, entity, criteria));
        }

        public void SubmitProcess(List<int> processKeys, List<JobCriteria> criteria, bool isManual, string triggeredBy)
        {
            var entity = CreateGroupEntity(0, criteria, isManual, triggeredBy);

            _bus.HandleWatchDogMessage(new GroupMessage(GroupActions.Start, entity, criteria, processKeys));
        }

        public void SubmitSingleProcess(int processKey, JobCriteria criteria, bool isManual=true, string triggeredBy="System")
        {
            SubmitProcess(new List<int>(){processKey}, new List<JobCriteria>(){criteria}, isManual, triggeredBy);
        }

        private IReadWritableGroupEntity CreateGroupEntity(int key, List<JobCriteria> criteria, bool isManual, string triggeredBy)
        {
            var serializer = _serializersFactory.GetSerializer<JobCriteria>();
            var crit = serializer.SerializeToString(criteria);

            IReadWritableGroupEntity entity = _entityFactory.CreateGroupEntity();

            //IGroupEntity entity =  _entityFactory.CreateGroupEntity(key, crit, isManual, false, triggeredBy, TaskCompletionStatus.Pending.Name);
            IWritableGroupEntity entityW = entity;

            entityW.Criteria = crit;
            entityW.GroupKey = key;
            entityW.IsManual = isManual;
            entityW.IsResubmission = false;
            entityW.SubmittedBy = triggeredBy;
            entityW.State = CompletionStatus.Pending.Name;

            //IGroupEntity g = entity;

            var  entity2 = _stateManager.CreateGroupEntity(entity);
            return entity2;
        }
    }
}