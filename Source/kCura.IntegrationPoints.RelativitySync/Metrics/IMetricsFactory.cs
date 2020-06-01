using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
	public interface IMetricsFactory
	{
		IMetric CreateScheduleJobStartedMetric(Job job);
		IMetric CreateScheduleJobCompletedMetric(Job job);
		IMetric CreateScheduleJobFailedMetric(Job job);
	}
}
