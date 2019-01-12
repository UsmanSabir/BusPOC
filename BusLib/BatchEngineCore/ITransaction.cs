using System;
using BusLib.Helper;

namespace BusLib.BatchEngineCore
{
    public interface ITransaction:IDisposable
    {
        void Commit();

        void Rollback();

        bool IsOpen(); //check if connection is open
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

        public bool IsOpen()
        {
            lock (this)
            {
                return _transaction?.IsOpen()??false;
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
            Robustness.Instance.SafeCall(() =>
            {
                _transaction?.Dispose();
                _transaction = null;
            });
        }
    }
}