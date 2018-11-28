using BusLib.Core;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore.Handlers
{
    internal class TaskProcessingPipeline:Pipeline<ITaskMessage>
    {
        private int _nodeThrotling = 0; //todo

        public TaskProcessingPipeline(ILogger logger, ITaskExecutorRepository taskRepository) : base(new TaskHandler(taskRepository))
        {
            var throttlingFilter = new TasksThrottlingFilter(_nodeThrotling, logger);
            RegisterFeatureDecorator(throttlingFilter);
            if (_nodeThrotling <= 0)
            {
                throttlingFilter.Disable();
            }

        }
    }
}