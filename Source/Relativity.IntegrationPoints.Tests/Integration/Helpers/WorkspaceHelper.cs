using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class WorkspaceHelper : HelperBase
	{
		public WorkspaceHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxy) : base(manager, database, proxy)
		{
		}

		public WorkspaceTest CreateWorkspace()
		{
			WorkspaceTest workspace = new WorkspaceTest();

			Database.Workspaces.Add(workspace);
			
			Database.Folders.Add(new FolderTest
			{
				WorkspaceId = workspace.ArtifactId,
				Name = workspace.Name
			});

			Database.Fields.Add(new FieldTest
			{
				WorkspaceId = workspace.ArtifactId,
				IsDocumentField = true,
				Name = "Control Number"
			});

			Database.SavedSearches.Add(new SavedSearchTest
			{
				WorkspaceId = workspace.ArtifactId,
				ParenObjectArtifactId = workspace.ArtifactId
			});

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
