using kCura.ScheduleQueue.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Telemetry.Metrics
{
	public interface IJobMetric
	{
		Task SendJobStartedAsync(Job job);
		Task SendJobCompletedAsync(Job job);
		Task SendJobFailedAsync(Job job);
	}
}
