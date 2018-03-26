using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using Relativity.Telemetry.APM;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Management.Tasks
{
	public class AlertStuckJobsTask : IManagementTask
	{
		private readonly IStuckJobs _stuckJobs;
		private readonly IAPM _apm;

		public AlertStuckJobsTask(IStuckJobs stuckJobs, IAPM apm)
		{
			_stuckJobs = stuckJobs;
			_apm = apm;
		}

		public void Run(IList<int> workspaceArtifactIds)
		{
			IDictionary<int, IList<JobHistory>> stuckJobs = _stuckJobs.FindStuckJobs(workspaceArtifactIds);

			if (stuckJobs != null && stuckJobs.Keys.Count > 0)
			{
				ICollection<int> stuckJobWorkspaceIds = stuckJobs.Keys;
				foreach (var workspaceId in stuckJobWorkspaceIds)
				{
					IList<JobHistory> stuckJobsForWorkspace = stuckJobs[workspaceId];
					IHealthMeasure healthCheck = _apm.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => HealthCheck.CreateStuckJobsMetric(workspaceId, stuckJobsForWorkspace));
					healthCheck.Write();
				}
			}
		}
	}
}