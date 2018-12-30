using System;
using BusLib.BatchEngineCore.Groups;

namespace BusLib.BatchEngineCore
{
    public interface IEntityFactory
    {
        //T Create<T>(Action<T> initilizer=null);


        IReadWritableGroupEntity CreateGroupEntity();


        IReadWritableProcessState CreateProcessEntity();

    }
}