using System;
using System.Collections.Generic;

namespace kCura.ScheduleQueue.Core
{
    public class TaskResult
    {
        public TaskStatusEnum Status { get; set; }
        public IEnumerable<Exception> Exceptions { get; set; } = new List<Exception>();
    }
}
