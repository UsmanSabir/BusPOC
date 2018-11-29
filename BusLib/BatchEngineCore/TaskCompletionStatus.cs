using BusLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public abstract class TaskCompletionStatus : Enumeration
    {
        public static List<TaskCompletionStatus> AllValues { get; } = new List<TaskCompletionStatus>();

        public static TaskCompletionStatus Pending = new PendingTaskStatus();
        public static TaskCompletionStatus Processing = new ProcessingTaskStatus();
        public static TaskCompletionStatus Finished = new FinishedTaskStatus();
        public static TaskCompletionStatus Stopped = new StoppedTaskStatus();

        public static bool IsDone(TaskCompletionStatus statue)
        {
            var isDone = statue.Id == Finished.Id || statue.Id == Stopped.Id;
            return isDone;
        }

        public static IEnumerable<TaskCompletionStatus> List()
        {
            return AllValues;// new[] { Pending, Processing, Finished};
        }

        #region Helper

        public static TaskCompletionStatus FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for TaskStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static TaskCompletionStatus From(int id)
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
        protected TaskCompletionStatus(int id, string name) : base(id, name)
        {
            AllValues.Add(this);
        } 
        #endregion

        #region TaskStatus Classes
        private class PendingTaskStatus : TaskCompletionStatus
        {
            public PendingTaskStatus() : base(1, "Pending")
            {

            }
        }

        private class ProcessingTaskStatus : TaskCompletionStatus
        {
            public ProcessingTaskStatus() : base(2, "Processing")
            {

            }
        }

        private class FinishedTaskStatus : TaskCompletionStatus
        {
            public FinishedTaskStatus() : base(3, "Finished")
            {

            }
        } 
        #endregion

        private class StoppedTaskStatus : TaskCompletionStatus
        {
            public StoppedTaskStatus() : base(4, "Stopped")
            {

            }
        }
    }
}
