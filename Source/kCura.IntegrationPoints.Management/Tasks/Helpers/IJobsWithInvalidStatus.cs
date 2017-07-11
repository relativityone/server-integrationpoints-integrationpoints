using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Management.Tasks.Helpers
{
	public interface IJobsWithInvalidStatus
	{
		IDictionary<int, IList<JobHistory>> Find(IList<int> workspaceArtifactIds);
	}
}