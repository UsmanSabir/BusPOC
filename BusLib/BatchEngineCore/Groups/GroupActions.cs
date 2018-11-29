using System;
using System.Collections.Generic;
using System.Linq;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Groups
{
    internal abstract class GroupActions : Enumeration
    {
        public static GroupActions Start = new StartStatus();
        public static GroupActions Stop = new StopStatus();
        

        public static IEnumerable<GroupActions> List()
        {
            return new[] { Start, Stop};
        }

        #region Helper

        public static GroupActions FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for ResultStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static GroupActions From(int id)
        {
            var state = List().SingleOrDefault(s => s.Id == id);

            if (state == null)
            {
                throw new ArgumentException($"Possible values for ResultStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        } 
        #endregion

        #region ctor
        protected GroupActions(int id, string name) : base(id, name)
        {
        } 
        #endregion

        #region Classes
        private class StartStatus : GroupActions
        {
            public StartStatus() : base(1, "Start")
            {

            }
        }

        private class StopStatus : GroupActions
        {
            public StopStatus() : base(2, "Stop")
            {

            }
        }

        #endregion
    }
}