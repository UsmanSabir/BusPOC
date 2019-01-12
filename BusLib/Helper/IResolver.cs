using System;

namespace BusLib.Helper
{
    public interface IResolver
    {
        T Resolve<T>();

        object Resolve(Type type);
    }

    public static class Resolver
    {
        public static IResolver Instance { get; set; }
    }
}