using System;
using System.Threading;
using BusLib.Core;

namespace BusLib.Helper
{
    public class Robustness
    {
        //private static Robustness instance;
        static readonly Lazy<Robustness> LazyRobustness = new Lazy<Robustness>();
        public static Robustness Instance { get; } = LazyRobustness.Value;

        public void SafeCall(Action action, ILogger logger = null)
        {
            SafeCallWithRetry(action, 0, 0, logger);
        }

        public void SafeCallWithRetry(Action action, int maxRetries, int delay = 1000, ILogger logger = null)
        {
            int currentRetry = 0;

            for (;;)
            {
                try
                {
                    action?.Invoke();
                    break;
                }
                catch (Exception ex)
                {
                    logger?.Warn($"Robustness.SafeCall has error '{ex.Message}'", ex);

                    currentRetry++;

                    // Check if the exception thrown was a transient exception
                    // based on the logic in the error detection strategy.
                    // Determine whether to retry the operation, as well as how
                    // long to wait, based on the retry strategy.
                    if (currentRetry > maxRetries)
                    {
                        return;
                    }
                }

                Thread.Sleep(delay);
            }
        }
    }
}