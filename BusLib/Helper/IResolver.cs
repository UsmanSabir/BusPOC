namespace BusLib.Helper
{
    public interface IResolver
    {
        T Resolve<T>();
    }

    public static class Resolver
    {
        public static IResolver Instance { get; set; }
    }
}