using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Groups;

namespace BusImpl
{
    public class EntityFactory : IEntityFactory
    {
        public IReadWritableGroupEntity CreateGroupEntity()
        {
            throw new System.NotImplementedException();
        }

        public IReadWritableProcessState CreateProcessEntity()
        {
            throw new System.NotImplementedException();
        }
    }
}