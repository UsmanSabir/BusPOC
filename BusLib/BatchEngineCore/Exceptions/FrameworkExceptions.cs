using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore.Exceptions
{
    //donot retry on framework exception
    public class FrameworkException : ApplicationException
    {        
        public FrameworkException(string errMsg):base(errMsg)
        {
            
        }
    }



    public class SagaStateException : FrameworkException
    {
        public SagaStateException(string errMsg):base(errMsg)
        {
            
        }
    }
}
