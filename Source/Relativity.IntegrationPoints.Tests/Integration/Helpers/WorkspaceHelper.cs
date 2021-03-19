using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class WorkspaceHelper : HelperBase
	{
		public WorkspaceTest SourceWorkspace { get; }

		public WorkspaceHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxy) : base(manager, database, proxy)
		{
			SourceWorkspace = CreateWorkspace();
		}

		public WorkspaceTest CreateWorkspace()
		{
			WorkspaceTest workspace = new WorkspaceTest();

			Database.Workspaces.Add(workspace);

			return workspace;
		}

		public void RemoveWorkspace(int workspaceId)
		{
			foreach (var workspace in Database.Workspaces.Where(x => x.ArtifactId == workspaceId).ToArray())
			{
				Database.Workspaces.Remove(workspace);
			}
		}
	}
}
