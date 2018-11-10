namespace BusLib.BatchEngineCore.VolumeAdapters
{
    public interface IVolumeAdapter<out T, in TU>
    {
        T Get(TU value);
    }
}