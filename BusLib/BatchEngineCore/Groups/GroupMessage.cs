namespace BusLib.BatchEngineCore.Groups
{
    internal class GroupMessage:IMessage
    {
        public GroupMessage(GroupActions action, IGroupEntity @group, string message=null)
        {
            Action = action;
            Group = @group;
            Message = message;
        }

        public GroupActions Action { get; }

        public IGroupEntity Group { get; }

        public string Message { get;  }
    }

    

}