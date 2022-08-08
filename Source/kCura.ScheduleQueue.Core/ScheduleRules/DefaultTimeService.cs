using System;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    public class DefaultTimeService : ITimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime LocalTime => UtcNow.ToLocalTime();
    }
}
