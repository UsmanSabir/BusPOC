using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusLib.Helper
{
    //https://codereview.stackexchange.com/a/160336
    public class Throttler
    {
        // Use this constant as average rate to disable throttling
        public const long NoLimit = -1;

        // Number of consumed tokens
        private long _consumedTokens;

        // timestamp of last refill time
        private long _lastRefillTime;

        // ticks per period
        private long _periodTicks;

        private double _averageRate;

        public long BurstSize
        {
            get;
            set;
        }

        public long AverageRate
        {
            get { return (long)_averageRate; }
            set { _averageRate = value; }
        }

        public TimeSpan Period
        {
            get
            {
                return new TimeSpan(_periodTicks);
            }
            set
            {
                _periodTicks = value.Ticks;
            }
        }

        public Throttler()
        {
            BurstSize = 1;
            AverageRate = NoLimit;
            Period = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Create a Throttler
        /// ex: To throttle to 1024 byte per seconds with burst of 200 byte use
        /// new Throttler(1024,TimeSpan.FromSeconds(1), 200);
        /// </summary>
        /// <param name="averageRate">The number of tokens to add to the bucket every interval. </param>
        /// <param name="period">Timespan of on interval.</param>
        /// <param name="burstSize"></param>
        public Throttler(long averageRate, TimeSpan period, long burstSize = 1)
        {
            BurstSize = burstSize;
            AverageRate = averageRate;
            Period = period;
        }

        public long TryThrottledWait(long amount)
        {
            if (BurstSize <= 0 || _averageRate <= 0)
            { // Instead of throwing exception, we just let all the traffic go
                return amount;
            }
            RefillToken();
            return ConsumeToken(amount);
        }

        // Return number of consummed token
        private long ConsumeToken(long amount)
        {
            while (true)
            {
                long currentLevel = Volatile.Read(ref _consumedTokens);
                long available = BurstSize - currentLevel;
                if (available == 0)
                {
                    return 0;
                }
                long toConsume = amount;
                if (available < toConsume)
                {
                    toConsume = available;
                }
                if (Interlocked.CompareExchange(ref _consumedTokens, currentLevel + toConsume, currentLevel) == currentLevel)
                {
                    return toConsume;
                }
            }
        }

        /// <summary>
        /// Wait that works inside synchronous methods. 
        /// </summary>
        /// <param name="amount">number of tokens to remove</param>
        /// <returns>Returns once all Thread.Sleep have occurred</returns>
        public void ThrottledWait(long amount)
        {
            long remaining = amount;
            while (true)
            {
                remaining -= TryThrottledWait(remaining);
                if (remaining == 0)
                {
                    break;
                }

                TimeSpan ts = GetSleepTime();
                Thread.Sleep(ts);
            }
        }

        /// <summary>
        /// Wait that works inside Async methods. 
        /// </summary>
        /// <param name="amount">number of tokens to remove</param>
        /// <returns>Returns once all Task.Delays have occurred</returns>
        public async Task ThrottledWaitAsync(long amount)
        {
            long remaining = amount;
            while (true)
            {
                remaining -= TryThrottledWait(remaining);
                if (remaining == 0)
                {
                    break;
                }

                TimeSpan ts = GetSleepTime();
                await Task.Delay(ts).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Compute elapsed time using DateTime.UtcNow.Ticks and refil token using _periodTicks and _averageRate
        /// </summary>
        private void RefillToken()
        {
            long currentTimeTicks = DateTime.UtcNow.Ticks;
            // Last refill time in  ticks unit
            long refillTime = Volatile.Read(ref _lastRefillTime);
            // Time delta in ticks unit
            long TicksDelta = currentTimeTicks - refillTime;
            long newTokens = (long)(TicksDelta * _averageRate / _periodTicks);
            if (newTokens <= 0)
            {
                return;
            }
            long newRefillTime = refillTime == 0
                    ? currentTimeTicks
                    : refillTime + (long)(newTokens * _periodTicks / _averageRate);
            // Only try to refill newTokens If no other thread has beaten us to the update _lastRefillTime  
            if (Interlocked.CompareExchange(ref _lastRefillTime, newRefillTime, refillTime) != refillTime)
            {
                return;
            }
            // Loop until we succeed in refilling "newTokens" tokens
            // Its still possible for 2 thread to concurrently run the block below
            // This is why we need to make sure the refill is atomic
            while (true)
            {
                long currentLevel = Volatile.Read(ref _consumedTokens);
                long adjustedLevel = Math.Min(currentLevel, BurstSize); // In case burstSize decreased
                long newLevel = Math.Max(0, adjustedLevel - newTokens);
                if (Interlocked.CompareExchange(ref _consumedTokens, newLevel, currentLevel) == currentLevel)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Get time to sleep until data can be sent again
        /// </summary>
        /// <returns>Timespan to wait</returns>
        private TimeSpan GetSleepTime()
        {
            long refillTime = Volatile.Read(ref _lastRefillTime);
            long nextRefillTime = (long)(refillTime + (_periodTicks / _averageRate));
            long currentTimeTicks = DateTime.UtcNow.Ticks;
            long sleepTicks = Math.Max(nextRefillTime - currentTimeTicks, 0);
            TimeSpan ts = new TimeSpan(sleepTicks);
            return ts;
        }
    }
}
