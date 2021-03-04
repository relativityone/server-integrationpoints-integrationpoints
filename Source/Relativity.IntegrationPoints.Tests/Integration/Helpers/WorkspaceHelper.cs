using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class WorkspaceHelper : HelperBase
	{
		public WorkspaceHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxy) : base(manager, database, proxy)
		{ }

		public WorkspaceTest CreateWorkspace()
		{
			WorkspaceTest workspace = new WorkspaceTest();

			Database.Workspaces.Add(workspace);

			ProxyMock.ObjectManager.SetupWorkspace(workspace);

			return workspace;
		}

		public void RemoveWorkspace(int workspaceId)
		{
			Database.Workspaces.RemoveAll(x => x.ArtifactId == workspaceId);
		}
	}
}
