using BusLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public abstract class CompletionStatus : Enumeration
    {
        public static List<CompletionStatus> AllValues { get; } = new List<CompletionStatus>();

        public static CompletionStatus Pending = new PendingTaskStatus();
        public static CompletionStatus Processing = new ProcessingTaskStatus();
        public static CompletionStatus Finished = new FinishedTaskStatus();
        public static CompletionStatus Stopped = new StoppedTaskStatus();

        public static bool IsDone(CompletionStatus statue)
        {
            var isDone = statue.Id == Finished.Id || statue.Id == Stopped.Id;
            return isDone;
        }

        public static IEnumerable<CompletionStatus> List()
        {
            return AllValues;// new[] { Pending, Processing, Finished};
        }

        #region Helper

        public static CompletionStatus FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for TaskStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static CompletionStatus From(int id)
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
        protected CompletionStatus(int id, string name) : base(id, name)
        {
            AllValues.Add(this);
        } 
        #endregion

        #region TaskStatus Classes
        private class PendingTaskStatus : CompletionStatus
        {
            public PendingTaskStatus() : base(1, "Pending")
            {

            }
        }

        private class ProcessingTaskStatus : CompletionStatus
        {
            public ProcessingTaskStatus() : base(2, "Processing")
            {

            }
        }

        private class FinishedTaskStatus : CompletionStatus
        {
            public FinishedTaskStatus() : base(3, "Finished")
            {

            }
        } 
        #endregion

        private class StoppedTaskStatus : CompletionStatus
        {
            public StoppedTaskStatus() : base(4, "Stopped")
            {

            }
        }
    }
}
