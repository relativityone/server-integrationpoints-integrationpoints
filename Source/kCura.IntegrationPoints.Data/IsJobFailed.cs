using System;

namespace kCura.IntegrationPoints.Data
{
    public class IsJobFailed
    {
        public Exception Exception { get; private set; }

        public bool ShouldBreakSchedule { get; private set; }

        public bool MaximumConsecutiveFailuresReached { get; private set; }

        public IsJobFailed(Exception exception, bool shouldBreakSchedule, bool maximumConsecutiveFailuresReached)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(Exception));
            ShouldBreakSchedule = shouldBreakSchedule;
            MaximumConsecutiveFailuresReached = maximumConsecutiveFailuresReached;
        }
    }
}
