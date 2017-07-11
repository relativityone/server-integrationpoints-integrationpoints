using System.Collections.Generic;

namespace kCura.IntegrationPoints.Management.Tasks
{
	public interface IManagementTask
	{
		void Run(IList<int> workspaceArtifactIds);
	}
}