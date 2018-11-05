using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib
{
     public class ActionCommand:ICommand
    {
        public ActionCommand(Action action)
        {
            Action = action;
        }

        public Action Action { get; }
    }
}
