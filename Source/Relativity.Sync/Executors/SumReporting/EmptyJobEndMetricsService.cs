﻿using System.Threading.Tasks;

namespace Relativity.Sync.Executors.SumReporting
{
	class EmptyJobEndMetricsService : IJobEndMetricsService
	{
		public Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}
