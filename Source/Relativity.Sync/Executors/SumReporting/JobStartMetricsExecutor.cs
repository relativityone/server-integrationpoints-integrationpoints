using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class JobStartMetricsExecutor : IExecutor<ISumReporterConfiguration>
	{
		private readonly ISyncMetrics _syncMetrics;

		public JobStartMetricsExecutor(ISyncMetrics syncMetrics)
		{
			_syncMetrics = syncMetrics;
		}

		public Task<ExecutionResult> ExecuteAsync(ISumReporterConfiguration configuration, CancellationToken token)
		{
			_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME);
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}