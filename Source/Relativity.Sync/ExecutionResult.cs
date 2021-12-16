using System;

namespace Relativity.Sync
{
	/// <summary>
	/// Result of the execution of a component of Sync.
	/// </summary>
	internal class ExecutionResult
	{
		/// <summary>
		/// Creates a <see cref="ExecutionResult"/> for a failed operation with the given exception.
		/// </summary>
		public static ExecutionResult Failure(Exception exception)
		{
			return new ExecutionResult(ExecutionStatus.Failed, exception.Message, exception);
		}

		/// <summary>
		/// Creates a <see cref="ExecutionResult"/> for a failed operation with the given message and exception.
		/// </summary>
		public static ExecutionResult Failure(string message, Exception exception)
		{
			return new ExecutionResult(ExecutionStatus.Failed, message, exception);
		}

		/// <summary>
		/// Creates a <see cref="ExecutionResult"/> for an operation that completed but encountered non-fatal
		/// errors during execution.
		/// </summary>
		public static ExecutionResult SuccessWithErrors(Exception exception)
		{
			return new ExecutionResult(ExecutionStatus.CompletedWithErrors, exception.Message, exception);
		}

		/// <summary>
		/// Creates a <see cref="ExecutionResult"/> for an operation that completed but encountered non-fatal
		/// errors during execution.
		/// </summary>
		public static ExecutionResult SuccessWithErrors()
		{
			return new ExecutionResult(ExecutionStatus.CompletedWithErrors, string.Empty, null);
		}

		/// <summary>
		/// Creates a <see cref="ExecutionResult"/> for a successful operation.
		/// </summary>
		public static ExecutionResult Success()
		{
			return new ExecutionResult(ExecutionStatus.Completed, string.Empty, null);
		}

		/// <summary>
		/// Creates a <see cref="ExecutionResult"/> for a cancelled operation.
		/// </summary>
		public static ExecutionResult Canceled()
		{
			return new ExecutionResult(ExecutionStatus.Canceled, string.Empty, null);
		}

		/// <summary>
		/// Creates a <see cref="ExecutionResult"/> for a cancelled operation.
		/// </summary>
		public static ExecutionResult Skipped()
		{
			return new ExecutionResult(ExecutionStatus.Skipped, string.Empty, null);
		}
		
		/// <summary>
		/// Creates a <see cref="ExecutionResult"/> for a paused operation.
		/// </summary>
		public static ExecutionResult Paused()
		{
			return new ExecutionResult(ExecutionStatus.Paused, string.Empty, null);
		}

		/// <summary>
		/// Status of the execution. <see cref="Exception"/> and <see cref="Message"/> will only have meaningful
		/// values when this property is not <see cref="ExecutionStatus.Completed"/>.
		/// </summary>
		public ExecutionStatus Status { get; }

		/// <summary>
		/// Error message from the failed execution.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Exception from the failed execution.
		/// </summary>
		public Exception Exception { get;  }

		internal ExecutionResult(ExecutionStatus status, string message, Exception exception)
		{
			Status = status;
			Message = message;
			Exception = exception;
		}
	}
}
