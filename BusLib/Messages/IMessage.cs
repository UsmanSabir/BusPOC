namespace BusLib
{
    public interface IMessage { }

    //public interface IRetryableMessage<T> : IMessage where T:IMessage
    //{
    //    bool IsRetry { get; }

    //    T Message { get; }
    //}

    //class RetryableMessage<T>:IRetryableMessage<T> where T : IMessage
    //{
    //    public RetryableMessage(T message)
    //    {
    //        Message = message;
    //    }

    //    public bool IsRetry { get; set; }
    //    public T Message { get; }
    //}
}
