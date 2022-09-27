using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Models
{
    public class StoppableJobHistoryCollection
    {
        public JobHistory[] PendingJobHistory { get; set; }

        public JobHistory[] ProcessingJobHistory { get; set; }

        public JobHistory[] ValidatingJobHistory { get; set; }

        public bool HasStoppableJobHistory
            => HasAny(PendingJobHistory) || HasAny(ProcessingJobHistory) || HasAny(ValidatingJobHistory);

        public bool HasOnlyPendingJobHistory
            => HasAny(PendingJobHistory) && !HasAny(ProcessingJobHistory);

        private bool HasAny(IEnumerable<JobHistory> jobHistory) => jobHistory?.Any() ?? false;
    }
}
