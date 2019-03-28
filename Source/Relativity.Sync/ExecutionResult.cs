using System;

namespace Relativity.Sync
{
	/// <summary>
	///     Result of the execution of a component of Sync.
	/// </summary>
	internal sealed class ExecutionResult
	{
		/// <summary>
		///     Creates a <see cref="ExecutionResult"/> for a failed operation with the given exception.
		/// </summary>
		public static ExecutionResult Failure(Exception exception)
		{
			return new ExecutionResult(ExecutionStatus.Failed, exception.Message, exception);
		}

		/// <summary>
		///     Creates a <see cref="ExecutionResult"/> for a successful operation.
		/// </summary>
		public static ExecutionResult Success()
		{
			return new ExecutionResult(ExecutionStatus.Completed, string.Empty, null);
		}

		/// <summary>
		///     Status of the execution. <see cref="Exception"/> and <see cref="Message"/> will only have meaningful values
		///     when this property is <see cref="ExecutionStatus.Failed"/>.
		/// </summary>
		public ExecutionStatus Status { get; }

		/// <summary>
		///     Error message from the failed execution.
		/// </summary>
		public string Message { get; }

		/// <summary>
		///     Exception from the failed execution.
		/// </summary>
		public Exception Exception { get; }

		private ExecutionResult(ExecutionStatus status, string message, Exception exception)
		{
			Status = status;
			Message = message;
			Exception = exception;
		}
	}
}
