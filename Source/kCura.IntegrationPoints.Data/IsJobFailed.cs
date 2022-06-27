using System;

namespace kCura.IntegrationPoints.Data
{
    public class IsJobFailed
    {
        public Exception Exception { get; private set; }

        public bool ShouldBreakSchedule { get; private set; }

        public IsJobFailed(Exception exception, bool shouldBreakSchedule)
        {
            Exception = exception;
            ShouldBreakSchedule = shouldBreakSchedule;
        }
    }
}
