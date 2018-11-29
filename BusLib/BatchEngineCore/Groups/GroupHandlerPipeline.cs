using BusLib.Core;

namespace BusLib.BatchEngineCore.Groups
{
    internal class GroupHandlerPipeline:Pipeline<GroupMessage>
    {
        public GroupHandlerPipeline() : base(new GroupCommandHandler())
        {

        }
    }
}