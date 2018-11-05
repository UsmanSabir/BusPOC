using System;
using System.Collections.Generic;
using System.Linq;

namespace CQRsConsole
{
    public class InMemoryBus : IBus
    {
        private static IDictionary<Type, Type> RegisteredSagas =
          new Dictionary<Type, Type>();
        private static IList<Type> RegisteredHandlers =
          new List<Type>();
        //private static IDictionary<string, Saga> RunningSagas =
        //  new Dictionary<string, Saga>();
        //void IBus.RegisterSaga<T>()
        //{
        //    var sagaType = typeof(T);
        //    var messageType = sagaType.GetInterfaces()
        //      .First(i => i.Name.StartsWith(typeof(IStartWith<>).Name))
        //      .GenericTypeArguments
        //      .First();
        //    RegisteredSagas.Add(messageType, sagaType);
        //}
        void IBus.Send<T>(T message)
        {
            SendInternal(message);
        }
        void IBus.RegisterHandler<T>()
        {
            RegisteredHandlers.Add(typeof(T));
        }
        void IBus.RaiseEvent<T>(T theEvent)
        {
            //https://github.com/NEventStore/NEventStore/wiki/Quick-Start
            //EventStore.Save(theEvent);
            SendInternal(theEvent);
        }
        void SendInternal<T>(T message) where T : Message
        {
            // Step 1: Launch sagas that start with given message
            // Step 2: Deliver message to all already running sagas that
            // match the ID (message contains a saga ID)
            // Step 3: Deliver message to registered handlers
        }
    }
}
