using System;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    public interface IScheduleRule
    {
        DateTime? GetNextUtcRunDateTime(DateTime lastNextRunTimeUtc);

        DateTime? GetFirstUtcRunDateTime();

        string ToSerializedString();

        int GetNumberOfContinuouslyFailedScheduledJobs();

        void IncrementConsecutiveFailedScheduledJobsCount();

        void ResetConsecutiveFailedScheduledJobsCount();
    }
}
