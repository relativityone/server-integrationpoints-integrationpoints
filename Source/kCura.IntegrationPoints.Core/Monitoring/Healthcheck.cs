using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public static class HealthCheck
	{
		public static HealthCheckOperationResult CreateJobFailedMetric()
		{
			return new HealthCheckOperationResult("Integration Points job failed!");
		}

		public static HealthCheckOperationResult CreateJobsWithInvalidStatusMetric(IDictionary<int, IList<JobHistory>> jobHistories)
		{
			var customData = jobHistories.ToDictionary(x => $"Workspace {x.Key}", y => (object) y.Value);
			return new HealthCheckOperationResult(false, "Jobs with invalid status found.", null, customData);
		}
	}
}