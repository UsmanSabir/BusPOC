namespace BusLib.BatchEngineCore
{
    public interface ITransaction
    {
        void Commit();

        void Rollback();
    }

    class TransactionWrapper:ITransaction
    {
        private ITransaction _transaction;

        public TransactionWrapper(ITransaction transaction)
        {
            _transaction = transaction;
        }


        public void Commit()
        {
            lock (this)
            {
                _transaction?.Commit();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            lock (this)
            {
                _transaction?.Rollback();
                _transaction = null;
            }
        }
    }
}