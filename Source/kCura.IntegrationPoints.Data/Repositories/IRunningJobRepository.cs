using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IRunningJobRepository
	{
		List<RDO> GetRunningJobs(int workspaceArtifactId);
	}
}