using System;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    public interface IScheduleRule
    {
        ITimeService TimeService { get; set; }
        string Description { get; }
        // int FailedScheduledJobsCount { get; set; }
        DateTime? GetNextUTCRunDateTime();
        string ToSerializedString();
    }
}
