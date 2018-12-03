using BusLib.Core;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore.Handlers
{
    internal class TaskProcessingPipeline:Pipeline<TaskMessage>
    {
        private readonly int _nodeThrottling = NodeSettings.Instance.Throttling; //todo

        public TaskProcessingPipeline(ILogger logger, ITaskExecutorsPool taskRepository) : base(new TaskHandler(taskRepository))
        {
            var throttlingFilter = new TasksThrottlingFilter(_nodeThrottling, logger);
            RegisterFeatureDecorator(throttlingFilter);
            if (_nodeThrottling <= 0)
            {
                throttlingFilter.Disable();
            }

        }
    }
}