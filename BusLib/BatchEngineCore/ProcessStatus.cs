//using System;
//using System.Collections.Generic;
//using System.Linq;
//using BusLib.Helper;

//namespace BusLib.BatchEngineCore
//{
//    public abstract class ProcessStatus : Enumeration
//    {
//        public static List<ProcessStatus> AllValues { get; } = new List<ProcessStatus>();

//        public static ProcessStatus New = new NewProcessStatus();
//        public static ProcessStatus InProgress = new InProgressProcessStatus();
//        public static ProcessStatus Finished = new FinishedProcessStatus();

//        public static IEnumerable<ProcessStatus> List()
//        {
//            return AllValues;// new[] { Pending, Processing, Finished};
//        }

//        #region Helper

//        public static ProcessStatus FromName(string name)
//        {
//            var state = List()
//                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

//            if (state == null)
//            {
//                throw new ArgumentException($"Possible values for ProcessStatus: {String.Join(",", List().Select(s => s.Name))}");
//            }

//            return state;
//        }

//        public static ProcessStatus From(int id)
//        {
//            var state = List().SingleOrDefault(s => s.Id == id);

//            if (state == null)
//            {
//                throw new ArgumentException($"Possible values for ProcessStatus: {String.Join(",", List().Select(s => s.Name))}");
//            }

//            return state;
//        } 
//        #endregion

//        #region ctor
//        protected ProcessStatus(int id, string name) : base(id, name)
//        {
//            AllValues.Add(this);
//        } 
//        #endregion

//        #region TaskStatus Classes
//        private class NewProcessStatus : ProcessStatus
//        {
//            public NewProcessStatus() : base(1, "New")
//            {

//            }
//        }

//        private class InProgressProcessStatus : ProcessStatus
//        {
//            public InProgressProcessStatus() : base(2, "InProgress")
//            {

//            }
//        }

//        private class FinishedProcessStatus : ProcessStatus
//        {
//            public FinishedProcessStatus() : base(9, "Finished")
//            {

//            }
//        }

//        //private class GeneratedProcessStatus : ProcessStatus
//        //{
//        //    public GeneratedProcessStatus() : base(3, "Generated")
//        //    {

//        //    }
//        //}

//        #endregion
//    }
//}