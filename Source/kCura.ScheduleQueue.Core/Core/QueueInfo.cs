using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Core
{
    public class QueueInfo
    {
        public int AllQueuedItems { get; set; }
        public int BlockedItems { get; set; }
        public int ItemsExcludedByTimeCondition { get; set; }
        public int ItemsExcludedBySyncWorkerPriorityRule { get; set; }
        public int WorkloadItems { get; set; }
    }
}
