using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class WorkspaceHelper : HelperBase
	{
		public WorkspaceHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxy) : base(manager, database, proxy)
		{ }

		public Workspace CreateWorkspace()
		{
			Workspace workspace = new Workspace();

			Database.Workspaces.Add(workspace);

			ProxyMock.ObjectManager.SetupWorkspace(workspace);

			return workspace;
		}
	}
}
