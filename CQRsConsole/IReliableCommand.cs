using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRsConsole
{
    interface IReliableCommand
    {
        void Execute();
    }

    class ReliableCommand : IReliableCommand
    {
        readonly Action _action;
        Action _onSuccessAction;
        Action _onErrorAction;

        public ReliableCommand(Action action, Action successAction = null, Action failAction=null)
        {
            this._action = action;
            this._onSuccessAction = successAction;
            this._onErrorAction = failAction;
        }

        public void Execute()
        {
            //try catch retry and send to que on fail
        }
    }
}
