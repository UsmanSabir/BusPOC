﻿using System.Threading;
using BusLib.BatchEngineCore;
using BusLib.Core;

namespace BusLib.PipelineFilters
{
    internal class TasksThrottlingFilter: FeatureCommandHandlerBase<TaskMessage>
    {
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphoreSlim;

        public TasksThrottlingFilter(int maxParallelRequest, ILogger logger)
        {
            _logger = logger;
            _semaphoreSlim = new SemaphoreSlim(maxParallelRequest);
        }

        public override void FeatureDecoratorHandler(TaskMessage message)
        {
            message.Logger.Trace("Entering throttling filter");

            try
            {
                _semaphoreSlim.Wait();
                message.OnCompleteActions.Register(() => _semaphoreSlim.Release());

                message.Logger.Trace("Leaving throttling filter");

                
                Handler?.Handle(message);
            }
            catch
            {
                throw;
            }
            finally
            {
                
            }
        }

        //public override TRes FeatureDecoratorQuery<TRes>(TaskMessage message)
        //{
        //    message.Logger.Trace("Entering throttling filter");

        //    try
        //    {
        //        _semaphoreSlim.Wait();
        //        message.OnCompleteActions.Register(() => _semaphoreSlim.Release());

        //        message.Logger.Trace("Leaving throttling filter");


        //        return Handler.Query<TRes>(message);
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {

        //    }
       // }
    }
}