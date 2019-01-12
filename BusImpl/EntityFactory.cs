using System;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Groups;

namespace BusImpl
{
    public class EntityFactory: IEntityFactory
    {
        
        public IReadWritableGroupEntity CreateGroupEntity()
        {
            //BatchGroupStateWrapper wrapper=new BatchGroupStateWrapper(new BatchGroupState());
            //return wrapper;
            throw new NotImplementedException();
        }

        public IReadWritableProcessState CreateProcessEntity()
        {
            IReadWritableProcessState wrapper = null;// new ProcessStateWrapper(new BatchProcessState());
            wrapper.Status= CompletionStatus.Pending;
            wrapper.Result= ResultStatus.Empty;
            return wrapper;
        }
    }
}