using System.Threading.Tasks;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Executors.SumReporting
{
    internal sealed class ImageJobSuspendedMetricsService : IJobEndMetricsService
    {
        private readonly ISyncMetrics _syncMetrics;

        public ImageJobSuspendedMetricsService(ISyncMetrics syncMetrics)
        {
            _syncMetrics = syncMetrics;
        }

        public Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
        {
            ImageJobSuspendedMetric imageJobSuspendedMetric = new ImageJobSuspendedMetric
            {
                JobSuspendedStatus = jobExecutionStatus.ToString()
            };
            _syncMetrics.Send(imageJobSuspendedMetric);
            return Task.FromResult(ExecutionResult.Success());
        }
    }
}
