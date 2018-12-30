using System;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Helper;

namespace BusLib.Core
{
    public abstract class RepeatingTriggeringProcess:SafeDisposable
    {
        /// <summary>
        /// The polling interval; default is five minutes.
        /// </summary>
        protected TimeSpan Interval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The logger to which I log.
        /// </summary>
        protected readonly ILogger Logger;

        readonly object _tokenLock = new object();

        /// <summary>
        /// A mechanism to cancel delays during shutdown.
        /// </summary>
        protected CancellationTokenSource Interrupter;
        private CancellationTokenSource _triggeringToken;
        public Task Completion { get; protected set; }
        protected bool IsActive { get; private set; } = false;
        /// <summary>
        /// The name by which I am known.
        /// </summary>
        public readonly string Name ;

        protected RepeatingTriggeringProcess(string name, ILogger logger)
        {
            Name = name;
            Logger = logger;
        }


        /// <summary>
        /// Do whatever it is I do, once.
        /// </summary>
        internal abstract void PerformIteration();

        internal virtual void OnStart()
        {

        }

        private async void PerformRepeatedly()
        {
            do
            {
                if (Interrupter.IsCancellationRequested)
                    break;

                var fred = Thread.CurrentThread;
                if (fred.Name == null)
                {
                    //Logger.Warn($"Anonymous thread {fred.ManagedThreadId} running as '{this.Name}'.");
                }

                try
                {
                    IsActive = true;

                    Robustness.Instance.SafeCall(
                        () => this.PerformIteration(),
                        this.Logger,
                        $"{this.Name} PerformIteration",
                        er => er is TaskCanceledException || er is OperationCanceledException);

                    var token = GetToken();

                    IsActive = false;
                    await Task.Delay(this.Interval, token);
                }
                catch (TaskCanceledException)
                {
                    // that's OK, that's what's supposed to happen.
                }
                catch (OperationCanceledException)
                {
                    // that's OK, that's what's supposed to happen.
                }
                IsActive = !Interrupter.IsCancellationRequested;
            } while (!Interrupter.IsCancellationRequested);

            IsActive = false;
            Logger.Trace($"Stopping thread {this.Name} immediately.");
        }

        
        CancellationToken GetToken()
        {
            lock (_tokenLock)
            {
                if (_triggeringToken == default || _triggeringToken.Token.IsCancellationRequested)
                {
                    _triggeringToken=CancellationTokenSource.CreateLinkedTokenSource(Interrupter.Token);
                }

                return _triggeringToken.Token;
            }
        }

        protected void Trigger()
        {
            lock (_tokenLock)
            {
                _triggeringToken?.CancelAfter(5);//todo : to make it async
            }
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

            Completion = Task.Factory.StartNew(PerformRepeatedly, parentToken, TaskCreationOptions.LongRunning, TaskScheduler.Default); //parentToken

            Robustness.Instance.SafeCall(OnStart, Logger);
        }

        protected void Stop()
        {
            Robustness.Instance.SafeCall(OnStopping);

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
            //Interrupter?.Dispose();
            //Interrupter = null;
            Completion = null;
        }

        protected virtual void OnStopping() { }

        protected override void Dispose(bool disposing)
        {
            Stop();
            
            base.Dispose(disposing);

        }
    }
}