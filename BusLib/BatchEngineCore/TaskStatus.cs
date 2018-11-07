using BusLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public abstract class TaskStatus : Enumeration
    {
        public static TaskStatus Pending = new PendingTaskStatus();
        public static TaskStatus Processing = new ProcessingTaskStatus();
        public static TaskStatus Finished = new FinishedTaskStatus();

        public static IEnumerable<TaskStatus> List()
        {
            return new[] { Pending, Processing, Finished};
        }

        #region Helper

        public static TaskStatus FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for TaskStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static TaskStatus From(int id)
        {
            var state = List().SingleOrDefault(s => s.Id == id);

            if (state == null)
            {
                throw new ArgumentException($"Possible values for TaskStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        } 
        #endregion

        #region ctor
        protected TaskStatus(int id, string name) : base(id, name)
        {
        } 
        #endregion

        #region TaskStatus Classes
        private class PendingTaskStatus : TaskStatus
        {
            public PendingTaskStatus() : base(1, "Pending")
            {

            }
        }

        private class ProcessingTaskStatus : TaskStatus
        {
            public ProcessingTaskStatus() : base(2, "Processing")
            {

            }
        }

        private class FinishedTaskStatus : TaskStatus
        {
            public FinishedTaskStatus() : base(3, "Finished")
            {

            }
        } 
        #endregion
    }
}
