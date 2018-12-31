using System.Collections.Generic;
using BusLib.BatchEngineCore;

namespace BusLib.JobSchedular
{
    public interface IJobScheduler //
    {
        long CreateJob(int groupKey, List<JobCriteria> criteria);

        long CreateJob(List<int> processKeys, List<JobCriteria> criteria);
    }
}