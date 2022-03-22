using System.Threading.Tasks;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Executors.SumReporting
{
	internal sealed class NonDocumentJobSuspendedMetricsService : IJobEndMetricsService
	{
		private readonly ISyncMetrics _syncMetrics;

		public NonDocumentJobSuspendedMetricsService(ISyncMetrics syncMetrics)
		{
			_syncMetrics = syncMetrics;
		}

		public Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
        {
            NonDocumentJobSuspendedMetric nonDocumentJobSuspendedMetric = new NonDocumentJobSuspendedMetric
			{
                JobSuspendedStatus = jobExecutionStatus.ToString()
            };
			_syncMetrics.Send(nonDocumentJobSuspendedMetric);
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}