using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IJobRepository
	{
		IList<RDO> GetRunningJobs(int workspaceArtifactId);
		IList<JobHistory> GetStuckJobs(IList<int> stuckJobsIds, int workspaceId);
	}
}