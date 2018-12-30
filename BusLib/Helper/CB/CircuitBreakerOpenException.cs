using System;

namespace BusLib.Helper.CB
{
    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException() { }

        public CircuitBreakerOpenException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
