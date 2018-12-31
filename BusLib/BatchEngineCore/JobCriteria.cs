using System;

namespace BusLib.BatchEngineCore
{
    public interface IJobCriteria
    {
        DateTime ProcessingDate { get; }
        int CompanyId { get; }
        int BranchId { get; }
        int SubTenantId { get; }
        int ReferenceId { get; }
        bool IsManual { get; }
        string Tag { get; }
    }

    public class JobCriteria : IJobCriteria
    {
        public DateTime ProcessingDate { get; set; }

        public int CompanyId { get; set; }

        public int BranchId { get; set; }

        public int SubTenantId { get; set; }

        public int ReferenceId { get; set; }

        public bool IsManual { get; set; }

        public string Tag { get; set; }

    }
}