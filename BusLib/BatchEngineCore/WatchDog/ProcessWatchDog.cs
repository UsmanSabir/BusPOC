using System;
using System.Linq;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core;

namespace BusLib.BatchEngineCore.WatchDog
{
    public class ProcessWatchDog:RepeatingProcess
    {
        private readonly IStateManager _stateManager;

        public ProcessWatchDog(ILogger logger, IStateManager stateManager) : base("WatchDog", logger)
        {
            _stateManager = stateManager;
            Interval=TimeSpan.FromSeconds(30);
        }


        internal override void PerformIteration()
        {
            var groups = _stateManager.GetAllIncomplete<IGroupEntity>().ToList();
            foreach (var groupEntity in groups)
            {
                var processes = _stateManager.GetPendingGroupProcess(groupEntity.Id).ToList();

            }


            //todo
            //get all pending groups, processes
            //check their tasks
        }

        //todo: any pub/sub triggering point
        void CheckProcessIdle(int processId)
        {
            //todo check if process tasks are finished
            //check deferred tasks
            //check process retry
            //complete process

            //get child processes or other pending group processes
            //send for volume generation
        }

    }
}