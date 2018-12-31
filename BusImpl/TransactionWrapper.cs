using BusLib.BatchEngineCore;

namespace BusImpl
{
    public class TransactionWrapper : ITransaction
    {
        public object TransactionObject => throw new System.NotImplementedException();

        public void Commit()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Rollback()
        {
            throw new System.NotImplementedException();
        }
    }
}