using System;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class PreValidationResult
    {
        public bool IsValid { get; private set; } = true;

        public bool ShouldExecute { get; private set; } = true;

        public bool ShouldError { get; private set; } = false;

        public bool ShouldBreakSchedule { get; private set; } = false;

        public bool MaximumConsecutiveFailuresReached { get; private set; } = false;

        public Exception Exception { get; private set; }

        public static PreValidationResult Success => new PreValidationResult();

        public static PreValidationResult InvalidJob(string message, bool shouldExecute, bool maximumConsecutiveFailuresReached) => new PreValidationResult
        {
            IsValid = false,
            ShouldExecute = shouldExecute,
            ShouldError = true,
            ShouldBreakSchedule = true,
            MaximumConsecutiveFailuresReached = maximumConsecutiveFailuresReached,
            Exception = new InvalidOperationException(message)
        };
    }
}
