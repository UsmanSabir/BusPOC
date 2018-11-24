namespace BusLib.BatchEngineCore
{
    public interface ITransaction
    {
        void Commit();

        void Rollback();
    }
}