using System;

namespace BusLib.BatchEngineCore
{
    public interface ITransaction:IDisposable
    {
        void Commit();

        void Rollback();

        object TransactionObject { get; }
    }

    class SafeTransactionWrapper:ITransaction
    {
        private ITransaction _transaction;

        public SafeTransactionWrapper(ITransaction transaction)
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

        public object TransactionObject
        {
            get
            {
                lock (this)
                {
                    var transactionObject = _transaction.TransactionObject;
                    return transactionObject;
                }
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
}