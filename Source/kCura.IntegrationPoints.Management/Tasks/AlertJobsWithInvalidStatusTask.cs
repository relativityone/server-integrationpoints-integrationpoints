using System.Collections.Generic;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Management.Tasks
{
	public class AlertJobsWithInvalidStatusTask : IManagementTask
	{
		private readonly IJobsWithInvalidStatus _jobsWithInvalidStatus;
		private readonly IAPM _apm;

		public AlertJobsWithInvalidStatusTask(IJobsWithInvalidStatus jobsWithInvalidStatus, IAPM apm)
		{
			_jobsWithInvalidStatus = jobsWithInvalidStatus;
			_apm = apm;
		}

		public void Run(IList<int> workspaceArtifactIds)
		{
			var invalidJobs = _jobsWithInvalidStatus.Find(workspaceArtifactIds);

			if (invalidJobs != null && invalidJobs.Keys.Count > 0)
			{
				_apm.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => HealthCheck.CreateJobsWithInvalidStatusMetric(invalidJobs));
			}
		}
	}
}