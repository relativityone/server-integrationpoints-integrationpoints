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
			Dictionary<string, object> customData = CreateJobHistoryCustomData(jobHistories);
			return new HealthCheckOperationResult(false, "Jobs with invalid status found.", null, customData);
		}

		public static HealthCheckOperationResult CreateStuckJobsMetric(IDictionary<int, IList<JobHistory>> jobHistories)
		{
			Dictionary<string, object> customData = CreateJobHistoryCustomData(jobHistories);
			return new HealthCheckOperationResult(false, "Stuck jobs found.", null, customData);
		}

		private static Dictionary<string, object> CreateJobHistoryCustomData(IDictionary<int, IList<JobHistory>> jobHistories)
		{
			return jobHistories.ToDictionary(x => $"Workspace {x.Key}", y => (object) string.Join(", ", y.Value.Select(z => $"Job {z.ArtifactId} in Workspace {y.Key}")));
		}
	}
}