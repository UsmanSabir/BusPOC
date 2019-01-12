using BusLib.BatchEngineCore;


namespace BusImpl
{
    public class TransactionWrapper:ITransaction
    {
        private readonly ITransaction _unitOfWork;

        public TransactionWrapper()
        {
            //_unitOfWork = unitOfWork;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        public void Commit()
        {
            _unitOfWork?.Commit();
        }

        public void Rollback()
        {
            _unitOfWork?.Dispose();
        }

        public bool IsOpen()
        {
            return _unitOfWork?.IsOpen() ?? false;
        }

        public object TransactionObject => _unitOfWork;
    }
}