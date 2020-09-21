using System;

namespace Relativity.Sync
{
	internal sealed class TaggingExecutionResult : ExecutionResult
	{
		public int TaggedDocumentsCount { get; set; }

		internal TaggingExecutionResult(ExecutionStatus status, string message, Exception exception) : base(status, message, exception)
		{
		}

		/// <summary>
		/// Creates a <see cref="TaggingExecutionResult"/> for a failed tagging operation with the given message and exception.
		/// </summary>
		public new static TaggingExecutionResult Failure(string message, Exception exception)
			=> new TaggingExecutionResult(ExecutionStatus.Failed, message, exception);

		/// <summary>
		/// Creates a <see cref="TaggingExecutionResult"/> for a successful operation.
		/// </summary>
		public new static TaggingExecutionResult Success()
			=> new TaggingExecutionResult(ExecutionStatus.Completed, string.Empty, null);
	}
}
