using System;

namespace kCura.ScheduleQueue.Core.Validation
{
	public class PreValidationResult
	{
		public bool IsValid { get; set; }

		public bool ShouldExecute { get; private set; } = true;

		public bool ShouldError { get; private set; } = false;

		public bool ShouldBreakSchedule { get; private set; } = false;

		public Exception Exception { get; private set; }

		public static PreValidationResult Success => new PreValidationResult();

		public static PreValidationResult InvalidJob(string message, bool shouldExecute) => new PreValidationResult
		{
			IsValid = false,
			ShouldExecute = shouldExecute,
			ShouldError = true,
			ShouldBreakSchedule = true,
			Exception = new InvalidOperationException(message)
		};

		public static PreValidationResult TimedOutJob(string message) => new PreValidationResult
		{
			IsValid = false,
			ShouldExecute = true,
			ShouldError = true,
			ShouldBreakSchedule = false,
			Exception = new TimeoutException(message)
		};
    }
}
