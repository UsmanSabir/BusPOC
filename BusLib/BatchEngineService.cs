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

        public BatchEngineService(IStateManager stateManager, IEntityFactory entityFactory)
        {
            _stateManager = stateManager;
            _entityFactory = entityFactory;
        }

        public void Start()
        {
            Bus.Instance.Start().Wait();
        }
        public void Stop()
        {
            Bus.Instance.Stop();
        }


        public void SubmitSingleProcess<T>(ProcessExecutionCriteria criteria, bool isManual = true, string triggeredBy = "System") where T: IBaseProcess
        {
            var processKey = Activator.CreateInstance<T>().ProcessKey;
            SubmitSingleProcess(processKey, criteria, isManual);
        }

        public void SubmitGroup(int groupKey, List<ProcessExecutionCriteria> criteria, bool isManual, string triggeredBy)
        {
            var entity = CreateGroupEntity(groupKey, criteria, isManual, triggeredBy);

            Bus.Instance.HandleWatchDogMessage(new GroupMessage(GroupActions.Start, entity, criteria));
        }

        public void SubmitProcess(List<int> processKeys, List<ProcessExecutionCriteria> criteria, bool isManual, string triggeredBy)
        {
            var entity = CreateGroupEntity(0, criteria, isManual, triggeredBy);

            Bus.Instance.HandleWatchDogMessage(new GroupMessage(GroupActions.Start, entity, criteria, processKeys));
        }

        public void SubmitSingleProcess(int processKey, ProcessExecutionCriteria criteria, bool isManual=true, string triggeredBy="System")
        {
            SubmitProcess(new List<int>(){processKey}, new List<ProcessExecutionCriteria>(){criteria}, isManual, triggeredBy);
        }

        private IReadWritableGroupEntity CreateGroupEntity(int key, List<ProcessExecutionCriteria> criteria, bool isManual, string triggeredBy)
        {
            var serializer = _serializersFactory.GetSerializer<ProcessExecutionCriteria>();
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