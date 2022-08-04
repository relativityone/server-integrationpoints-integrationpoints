using kCura.IntegrationPoints.Data;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Models
{
    public class StoppableJobHistoryCollection
    {
        public JobHistory[] PendingJobHistory { get; set; } 
        public JobHistory[] ProcessingJobHistory { get; set; }

        public bool HasStoppableJobHistory
            => HasAny(PendingJobHistory) || HasAny(ProcessingJobHistory);

        public bool HasOnlyPendingJobHistory
            => HasAny(PendingJobHistory) && !HasAny(ProcessingJobHistory);

        private bool HasAny(IEnumerable<JobHistory> jobHistory) => jobHistory?.Any() ?? false;
    }
}