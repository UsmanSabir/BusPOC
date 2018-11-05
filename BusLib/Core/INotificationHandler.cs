using BusLib.Messages;

namespace BusLib.Core
{
    public interface INotificationHandler<in TRequest> where TRequest : INotification
    {
        void Handle(TRequest notification);
    }
}
