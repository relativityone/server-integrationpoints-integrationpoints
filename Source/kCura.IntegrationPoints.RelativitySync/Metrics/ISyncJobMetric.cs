using kCura.ScheduleQueue.Core;
using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
	public interface ISyncJobMetric
	{
		Task SendJobStartedAsync(Job job);
		Task SendJobCompletedAsync(Job job);
		Task SendJobFailedAsync(Job job, Exception e);
	}
}
