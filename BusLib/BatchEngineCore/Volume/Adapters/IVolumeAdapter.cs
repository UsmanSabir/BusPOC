namespace BusLib.BatchEngineCore.Volume.Adapters
{
    public interface IVolumeAdapter<out T, in TU>
    {
        T Get(TU value);
    }
}