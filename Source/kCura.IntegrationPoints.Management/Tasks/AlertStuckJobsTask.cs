using System.Collections.Generic;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using Relativity.Telemetry.APM;

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
			var stuckJobs = _stuckJobs.FindStuckJobs(workspaceArtifactIds);

			if (stuckJobs != null && stuckJobs.Keys.Count > 0)
			{
				_apm.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => HealthCheck.CreateStuckJobsMetric(stuckJobs));
			}
		}
	}
}