using BusLib.Core;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore.Volume
{
    internal class DbActionCommandHandler : IHandler<Infrastructure.DbAction>
    {
        public void Handle(Infrastructure.DbAction message)
        {
            if (message.Action == DbActions.Persist)
            {
                message.Command?.Invoke();
            }
            else if (message.Action == DbActions.Transaction)
            {
                message.Command?.Invoke();
            }
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