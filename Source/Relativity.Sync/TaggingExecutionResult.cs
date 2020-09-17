using System;

namespace Relativity.Sync
{
	internal class TaggingExecutionResult : ExecutionResult
	{
		public int TaggedDocumentsCount { get; set; }

		internal TaggingExecutionResult(ExecutionStatus status, string message, Exception exception) : base(status, message, exception)
		{
		}
	}
}
