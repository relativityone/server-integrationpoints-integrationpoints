using System;

namespace kCura.ScheduleQueue.Core.Core
{
    public class TaskParameters
    {
        public Guid BatchInstance { get; set; }

        public object BatchParameters{ get; set; }

        public long? BatchStartingIndex { get; set; }
    }
}
