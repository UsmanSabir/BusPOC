using System;
using Autofac;
using BusLib.Helper;

namespace BusImpl
{
    public class ContainerResolver: IResolver
    {
        private readonly IContainer _container;

        public ContainerResolver(IContainer container)
        {
            this._container = container;
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }
    }
}