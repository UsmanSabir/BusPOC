using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Infrastructure
{
    class ActionCommand : ICommand
    {
        public ActionCommand(Action action)
        {
            Action = action;
        }

        public Action Action { get; private set; }
    }

    public class DbAction:IMessage
    {
        public DbAction(DbActions action, Action command=null)
        {
            Action = action;
            Command = command;
        }

        public DbActions Action { get; private set; }

        public Action Command { get; private set; }
    }

    public enum DbActions
    {
        Opening,
        Opened,
        Error,
        Executing,
        Executed,
        Closed,
        Persist,
        Transaction
    }
}
