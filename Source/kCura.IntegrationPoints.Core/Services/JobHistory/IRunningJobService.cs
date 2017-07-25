using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public interface IRunningJobService
	{
		List<RDO> GetRunningJobs(int workspaceArtifactId);
	}
}