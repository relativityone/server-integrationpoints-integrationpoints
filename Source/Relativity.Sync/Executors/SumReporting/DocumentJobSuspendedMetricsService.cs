using System.Threading.Tasks;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Executors.SumReporting
{
	internal sealed class DocumentJobSuspendedMetricsService : IJobEndMetricsService
	{
		private readonly ISyncMetrics _syncMetrics;

		public DocumentJobSuspendedMetricsService(ISyncMetrics syncMetrics)
		{
			_syncMetrics = syncMetrics;
		}

		public Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
        {
            DocumentJobSuspendedMetric documentJobSuspendedMetric = new DocumentJobSuspendedMetric
            {
                JobSuspendedStatus = jobExecutionStatus.ToString()
            };
			_syncMetrics.Send(documentJobSuspendedMetric);
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}