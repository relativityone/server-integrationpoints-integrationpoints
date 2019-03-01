using System;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class StepResult
	{
		public StepResult(CommandExecutionStatus executionStatus, TimeSpan duration)
		{
			ExecutionStatus = executionStatus;
			Duration = duration;
		}

		public CommandExecutionStatus ExecutionStatus { get; }

		public TimeSpan Duration { get; }
	}
}