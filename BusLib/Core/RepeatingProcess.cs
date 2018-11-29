using System;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Helper;

namespace BusLib.Core
{
    public abstract class RepeatingProcess:SafeDisposable
    {
        /// <summary>
        /// The polling interval; default is five minutes.
        /// </summary>
        protected TimeSpan Interval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The logger to which I log.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// A mechanism to cancel delays during shutdown.
        /// </summary>
        protected CancellationTokenSource Interrupter;

        public Task Completion { get; protected set; }

        /// <summary>
        /// The name by which I am known.
        /// </summary>
        public readonly string Name ;

        protected RepeatingProcess(string name, ILogger logger)
        {
            Name = name;
            Logger = logger;
        }


        /// <summary>
        /// Do whatever it is I do, once.
        /// </summary>
        internal abstract void PerformIteration();

        private async void PerformRepeatedly()
        {
            do
            {
                if(Interrupter.IsCancellationRequested)
                    break;
                
                var fred = Thread.CurrentThread;
                if (fred.Name == null)
                {
                    Logger.Warn($"Anonymous thread {fred.ManagedThreadId} running as '{this.Name}'.");
                }

                Robustness.Instance.SafeCall(
                    () => this.PerformIteration(),
                    this.Logger,
                    $"{this.Name} PerformIteration");
                try
                {
                    await Task.Delay(this.Interval, Interrupter.Token);
                }
                catch (TaskCanceledException)
                {
                    // that's OK, that's what's supposed to happen.
                }
                catch (OperationCanceledException)
                {
                    // that's OK, that's what's supposed to happen.
                }
            } while (!Interrupter.IsCancellationRequested);

            Logger.Trace($"Stopping thread {this.Name} immediately.");
        }

        public void Start(CancellationToken parentToken = default(CancellationToken))
        {
            if (Completion != null)
            {
                throw new InvalidOperationException("Already running");
            }

            if (parentToken == default(CancellationToken))
                Interrupter = new CancellationTokenSource();
            else
                Interrupter = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

            Completion = Task.Factory.StartNew(PerformRepeatedly); //parentToken
        }

        protected void Stop()
        {
            Interrupter?.Cancel();
            try
            {
                Completion?.Wait();
            }
            catch (AggregateException e)
            {
                //that's OK
                e.Handle(ex =>
                {
                    Logger.Trace( $"Error handled while stopping Repeating process {Name} with message {ex.Message}");
                    return true;
                });
            }
            Interrupter?.Dispose();
            Interrupter = null;
            Completion = null;
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            
            base.Dispose(disposing);

        }
    }
}