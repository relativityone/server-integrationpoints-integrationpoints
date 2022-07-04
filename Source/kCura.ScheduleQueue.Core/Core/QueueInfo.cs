using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Core
{
    public class QueueInfo
    {
        public int AllQueuedItemsCount { get; set; }
        public int BlockedJobsCount { get; set; }
        public int JobsExcludedByTimeConditionCount { get; set; }
        public int JobsExcludedBySyncWorkerPriorityCount { get; set; }
        public int TotalWorkloadCount { get; set; }
    }
}
