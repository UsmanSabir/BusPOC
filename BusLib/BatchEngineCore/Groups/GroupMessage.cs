using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using BusLib.BatchEngineCore.PubSub;

namespace BusLib.BatchEngineCore.Groups
{
    internal class GroupMessage:IWatchDogMessage
    {
        private IReadWritableGroupEntity _group;

        public GroupMessage()
        {
            
        }

        public GroupMessage(GroupActions action, IReadWritableGroupEntity entity, List<ProcessExecutionCriteria> criteria, string message=null)
        {
            Action = action;
            Group = entity;
            Criteria = criteria;
            Message = message;
            GroupId = entity.Id;
        }

        public GroupMessage(GroupActions action, IReadWritableGroupEntity entity, List<ProcessExecutionCriteria> criteria, List<int> processKeys)
        {
            Action = action;
            Group = entity;
            GroupId = entity.Id;
            Criteria = criteria;
            ProcessKeys = processKeys;
            IsProcessSubmission = true;
        }

        public List<int> ProcessKeys
        {
            get;
            set;
        }

        public bool IsProcessSubmission { get; set; } = false;


        public GroupActions Action { get; set; }

        public long GroupId { get; set; }

        [XmlIgnore]
        //[field: NonSerialized]
        //[ScriptIgnore]
        internal IReadWritableGroupEntity Group
        {
            get => _group;
            set => _group = value;
        }

        public string Message { get; set; }

        public List<ProcessExecutionCriteria> Criteria { get; set; }

        [XmlIgnore]
        public object Sender { get; set; }

        public void SetGroupEntity(IReadWritableGroupEntity groupEntity)
        {
            Group = groupEntity;
        }
    }

    

}
