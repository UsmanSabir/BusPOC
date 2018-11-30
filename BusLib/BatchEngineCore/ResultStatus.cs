using BusLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public abstract class ResultStatus : Enumeration
    {
        public static ResultStatus Success = new SuccessResultStatus();
        public static ResultStatus Error = new ErrorResultStatus();
        public static ResultStatus Invalid = new InvalidResultStatus();
        public static ResultStatus Empty = new EmptyResultStatus();

        public static IEnumerable<ResultStatus> List()
        {
            return new[] { Success, Error, Invalid};
        }

        #region Helper

        public static ResultStatus FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for ResultStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static ResultStatus From(int id)
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
        protected ResultStatus(int id, string name) : base(id, name)
        {
        } 
        #endregion

        #region Classes
        private class SuccessResultStatus : ResultStatus
        {
            public SuccessResultStatus() : base(1, "Success")
            {

            }
        }

        private class ErrorResultStatus : ResultStatus
        {
            public ErrorResultStatus() : base(2, "Error")
            {

            }
        }

        private class InvalidResultStatus : ResultStatus
        {
            public InvalidResultStatus() : base(3, "Invalid")
            {

            }
        }

        private class EmptyResultStatus : ResultStatus
        {
            public EmptyResultStatus() : base(4, "Empty")
            {

            }
        }
        #endregion
    }
}
