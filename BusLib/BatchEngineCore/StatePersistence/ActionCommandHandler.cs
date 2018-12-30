using BusLib.Core;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore.Volume
{
    internal class ActionCommandHandler: IHandler<Infrastructure.ActionCommand>
    {
        public void Handle(Infrastructure.ActionCommand message)
        {
            message.Action?.Invoke();
        }

        //public TResult Query<TResult>(Infrastructure.ActionCommand message)
        //{
        //    throw new System.NotImplementedException();
        //}

        public void Dispose()
        {
            
        }
    }
}