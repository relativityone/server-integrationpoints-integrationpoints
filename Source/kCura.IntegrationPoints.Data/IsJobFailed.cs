using System;

namespace kCura.IntegrationPoints.Data
{
    public class IsJobFailed
    {
        public Exception Exception { get; }

        public bool ShouldBreakSchedule { get; }

        public bool MaximumConsecutiveFailuresReached { get; }

        public IsJobFailed(Exception exception, bool shouldBreakSchedule, bool maximumConsecutiveFailuresReached)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(Exception));
            ShouldBreakSchedule = shouldBreakSchedule;
            MaximumConsecutiveFailuresReached = maximumConsecutiveFailuresReached;
        }
    }
}
