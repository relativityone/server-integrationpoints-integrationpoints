using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class WorkspaceHelper : HelperBase
	{
		public WorkspaceHelper(InMemoryDatabase database, ProxyMock proxy) : base(database, proxy)
		{ }

		public Workspace CreateWorkspace(string name)
		{
			Workspace workspace = new Workspace
			{
				ArtifactId = Artifact.Next(),
				Name = name
			};

			Database.Workspaces.Add(workspace);

			ProxyMock.ObjectManager.SetupWorkspace(workspace);

			return workspace;
		}
	}
}
