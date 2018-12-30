using System;
using System.Threading;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore.Volume
{
    internal class CacheStoragePipeline: ReliablePipeline<Infrastructure.ActionCommand>
    {
        private readonly ILogger _logger;
        private readonly IProcessDataStorage _originalStorage;

        public CacheStoragePipeline(ILogger logger, CancellationToken token, IProcessDataStorage originalStorage) 
            : base(new ActionCommandHandler(), "CacheStoragePipeline", logger, 9, 500, token,
                exception =>
                {
                    logger.Warn($"Error in Cache pipeline. {exception.Message}", exception);
                    Robustness.Instance.ExecuteUntilTrue(originalStorage.RefreshIfNotHealth, token);
                })
        {
            _logger = logger;
            _originalStorage = originalStorage;
            //IProcessDataStorage
        }
        
    }
}