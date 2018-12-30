using System;

namespace BusLib.BatchEngineCore
{
    public class ProcessExecutionCriteria
    {
        public DateTime ProcessingDate { get; set; }

        public int CompanyId { get; set; }

        public int BranchId { get; set; }

        public int SubTenantId { get; set; }

        public int ReferenceId { get; set; }

        public string Tag { get; set; }

    }
}