using System;
using System.Linq;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core;

namespace BusLib.BatchEngineCore.WatchDog
{
    public class ProcessWatchDog:RepeatingProcess
    {
        private IStateManager _stateManager;

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
    }
}