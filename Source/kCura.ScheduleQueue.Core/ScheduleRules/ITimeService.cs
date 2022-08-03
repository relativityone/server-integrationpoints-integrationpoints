using System;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    public interface ITimeService
    {
        DateTime UtcNow { get; }

        DateTime LocalTime { get; }
    }
}
