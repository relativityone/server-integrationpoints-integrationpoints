using kCura.IntegrationPoints.Core.Telemetry.Metrics;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Telemetry.Metrics
{
	public interface IMetricsFactory
	{
		IMetric CreateScheduleJobStartedMetric(Job job);
		IMetric CreateScheduleJobCompletedMetric(Job job);
		IMetric CreateScheduleJobFailedMetric(Job job);
	}
}
