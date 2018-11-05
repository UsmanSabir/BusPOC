using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public class CommandHandler : IHandler<ICommand>
    {
        public void Handle(ICommand command)
        {
            command.Action();
        }
    }
}
