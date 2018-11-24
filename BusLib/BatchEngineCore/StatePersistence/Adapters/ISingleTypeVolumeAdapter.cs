namespace BusLib.BatchEngineCore.Volume.Adapters
{
    /// <summary>
    /// Streaming volume adapter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISingleTypeVolumeAdapter<T>:IVolumeAdapter<T,T>
    {
        
    }
}