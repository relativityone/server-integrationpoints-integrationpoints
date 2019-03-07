using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using Relativity.Telemetry.APM;
using Constants = kCura.IntegrationPoints.Core.Constants;

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
			IDictionary<int, IList<JobHistory>> invalidJobs = new Dictionary<int, IList<JobHistory>>();

			if (invalidJobs != null && invalidJobs.Keys.Count > 0)
			{
				var healthCheck = _apm.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => HealthCheck.CreateJobsWithInvalidStatusMetric(invalidJobs));
				healthCheck.Write();
			}
		}
	}
}