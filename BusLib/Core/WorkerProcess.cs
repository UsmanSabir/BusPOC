using System;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Helper;

namespace BusLib.Core
{
    public abstract class WorkerProcess:SafeDisposable
    {
        
        /// <summary>
        /// The logger to which I log.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// A mechanism to cancel delays during shutdown.
        /// </summary>
        protected CancellationTokenSource Interrupter;

        
        /// <summary>
        /// The name by which I am known.
        /// </summary>
        public readonly string Name ;

        protected WorkerProcess(string name, ILogger logger)
        {
            Name = name;
            Logger = logger;
        }


        /// <summary>
        /// Do whatever it is I do, once.
        /// </summary>
        internal abstract void OnStart();
        internal abstract void OnStop();
        
        public void Start(CancellationToken parentToken = default(CancellationToken))
        {
            if (Interrupter != null)
            {
                throw new InvalidOperationException("Already running");
            }

            if (parentToken == default(CancellationToken))
                Interrupter = new CancellationTokenSource();
            else
                Interrupter = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

            try
            {
                OnStart();
            }
            catch (Exception e)
            {
                Logger.Error($"Error starting worker '{Name}'. Message {e.Message}", e);
                Interrupter.Cancel();
                Interrupter = null;
                throw;
            }
        }

        protected void Stop()
        {
            Interrupter?.Cancel();
            try
            {
                OnStop();
            }
            catch (TaskCanceledException)
            {
                // that's OK, that's what's supposed to happen.
            }
            catch (OperationCanceledException)
            {
                // that's OK, that's what's supposed to happen.
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
            Interrupter = null;
            Logger.Trace($"Worker process '{Name}' stopped");
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            
            base.Dispose(disposing);

        }
    }
}