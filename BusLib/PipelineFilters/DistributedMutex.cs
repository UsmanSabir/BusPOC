using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Core;

namespace BusLib.PipelineFilters
{
    public abstract class DistributedMutex
    {
        private static readonly TimeSpan RenewInterval = TimeSpan.FromSeconds(45);
        private static readonly TimeSpan AcquireAttemptInterval = TimeSpan.FromSeconds(65);
        private readonly string _key;
        private readonly Func<CancellationToken, Task> _taskToRunWhenLockAcquired;
        private readonly ILogger _logger;

        protected DistributedMutex(string key, Func<CancellationToken, Task> taskToRunWhenLockAcquired, ILogger logger)
        {
            _key = key;
            _taskToRunWhenLockAcquired = taskToRunWhenLockAcquired;
            _logger = logger;
        }

        public async Task RunTaskWhenMutexAcquired(CancellationToken token)
        {
            await RunTaskWhenLockAcquired(token);
        }

        private async Task RunTaskWhenLockAcquired(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Try to acquire the lock, otherwise wait for some time before we can try again.
                string lockId = await TryAcquireLockOrWait(token);

                if (!string.IsNullOrEmpty(lockId))
                {
                    // Create a new linked cancellation token source, so if either the 
                    // original token is canceled or the lock cannot be renewed,
                    // then the leader task can be canceled.
                    using (var lockCts = CancellationTokenSource.CreateLinkedTokenSource(new[] { token }))
                    {
                        // Run the leader task.
                        var leaderTask = _taskToRunWhenLockAcquired.Invoke(lockCts.Token);

                        // Keeps renewing the lock in regular intervals. 
                        // If the lock cannot be renewed, then the task completes.
                        var renewLockTask = KeepRenewingLock(lockId, lockCts.Token);

                        // When any task completes (either the leader task or when it could
                        // not renew the lock) then cancel the other task.
                        await CancelAllWhenAnyCompletes(leaderTask, renewLockTask, lockCts);
                    }
                }
            }
        }

        private async Task CancelAllWhenAnyCompletes(Task leaderTask, Task renewLockTask, CancellationTokenSource cts)
        {
            await Task.WhenAny(leaderTask, renewLockTask);

            // Cancel the user's leader task or the renewlock Task, as it is no longer the leader.
            cts.Cancel();

            var allTasks = Task.WhenAll(leaderTask, renewLockTask);
            try
            {
                await Task.WhenAll(allTasks);
            }
            catch (Exception)
            {
                if (allTasks.Exception != null)
                {
                    allTasks.Exception.Handle(ex =>
                    {
                        if (!(ex is OperationCanceledException))
                        {
                            _logger.Error(ex.Message);
                        }

                        return true;
                    });
                }
            }
        }
        
        private async Task<string> TryAcquireLockOrWait(CancellationToken token)
        {
            try
            {
                var lockId = await AcquireLockAsync(_key, token);
                if (!string.IsNullOrEmpty(lockId))
                {
                    return lockId;
                }

                await Task.Delay(AcquireAttemptInterval, token);
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        private async Task KeepRenewingLock(string lockId, CancellationToken token)
        {
            var renewOffset = new Stopwatch();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Immediately attempt to renew the lock
                    // We cannot be sure how much time has passed since the lock was actually acquired
                    renewOffset.Restart();
                    var renewed = await RenewLockAsync(lockId, token);
                    renewOffset.Stop();

                    if (!renewed)
                    {
                        return;
                    }

                    // We delay based on the time from the start of the last renew request to ensure
                    var renewIntervalAdjusted = RenewInterval - renewOffset.Elapsed;

                    // If the adjusted interval is greater than zero wait for that long
                    if (renewIntervalAdjusted > TimeSpan.Zero)
                    {
                        await Task.Delay(RenewInterval - renewOffset.Elapsed, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    ReleaseLock(lockId);

                    return;
                }
            }
        }

        protected abstract Task<string> AcquireLockAsync(string key, CancellationToken token);

        protected abstract void ReleaseLock(string lockId);

        protected abstract Task<bool> RenewLockAsync(string lockId, CancellationToken token);
    }
}