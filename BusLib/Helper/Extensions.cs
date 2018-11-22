using System;
using BusLib.BatchEngineCore;

namespace BusLib.Helper
{
    public static class Extensions
    {
        public static int ToInt32Timeout(this TimeSpan timeout, string paramName = null)
        {
            // based on http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,959427ac16fa52fa

            var totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(paramName ?? "timeout");
            }

            return (int)totalMilliseconds;
        }

        public static void MarkAsError(this IProcessExecutionContext context, string errorMessage)
        {
            context.Logger.Error(errorMessage);
            //todo: send to stateManager queue
        }

        public static void MarkAsVolumeGenerated(this IProcessExecutionContext context)
        {
            //todo: send to stateManager queue
        }

        internal static string GetFormatedLogMessage(this IProcessExecutionContext context, string msg,
            Exception exception=null)
        {
            return
                $"{msg ?? string.Empty} for process Id: {context.ProcessState.Id}, Key: {context.ProcessState.ProcessKey}, CorrelationId: {context.CorrelationId}{(exception!=null?exception.ToString():string.Empty)}";
        }
    }
}