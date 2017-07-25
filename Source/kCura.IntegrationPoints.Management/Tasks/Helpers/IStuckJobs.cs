using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Management.Tasks.Helpers
{
	public interface IStuckJobs
	{
		IDictionary<int, IList<JobHistory>> FindStuckJobs(IList<int> workspaceArtifactIds);
	}
}