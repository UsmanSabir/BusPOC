using System;

namespace BusLib.Core
{
    public interface IHandler<in T>: IDisposable where T : IMessage
    {
        void Handle(T message);
    }

    public interface IFeatureHandler<T> : IHandler<T> where T : IMessage
    {
        IFeatureHandler<T> Register(IHandler<T> handler);
        void FeatureDecoratorHandler(T message);
        void Enable();
        void Disable();
        bool IsEnabled { get; }
    }
}