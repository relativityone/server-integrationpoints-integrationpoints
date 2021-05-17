using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers
{
	public class WorkspaceHelper : RelativityHelperBase
	{
		private readonly ProxyMock _proxy;
		private readonly ISerializer _serializer;

		public WorkspaceHelper(RelativityInstanceTest relativity, ProxyMock proxy, ISerializer serializer) : base(relativity)
		{
			_proxy = proxy;
			_serializer = serializer;
		}
		

		public WorkspaceTest CreateWorkspace(int? workspaceArtifactId = null)
		{
			WorkspaceTest workspace = new WorkspaceTest(_serializer, workspaceArtifactId);

			Relativity.Workspaces.Add(workspace);
			
			workspace.Folders.Add(new FolderTest
			{
				Name = workspace.Name
			});

			workspace.Fields.Add(new FieldTest
			{
				IsDocumentField = true,
				IsIdentifier = true,
				Name = "Control Number"
			});

			workspace.SavedSearches.Add(new SavedSearchTest
			{
				ParenObjectArtifactId = workspace.ArtifactId,
				Name = "All Documents"
			});
		
			return workspace;
		}

		public WorkspaceTest CreateWorkspaceWithIntegrationPointsApp(int? workspaceArtifactId)
		{
			WorkspaceTest workspace = CreateWorkspace(workspaceArtifactId);

			workspace.Helpers.SourceProviderHelper.CreateLDAP();
			workspace.Helpers.SourceProviderHelper.CreateFTP();
			workspace.Helpers.SourceProviderHelper.CreateLoadFile();
			workspace.Helpers.SourceProviderHelper.CreateRelativity();

			workspace.Helpers.DestinationProviderHelper.CreateRelativityProvider();
			
			workspace.Helpers.DestinationProviderHelper.CreateLoadFile();
			
			workspace.Helpers.IntegrationPointTypeHelper.CreateImportType();
			workspace.Helpers.IntegrationPointTypeHelper.CreateExportType();
		
			return workspace;
		}

		public void RemoveWorkspace(int workspaceId)
		{
			foreach (var workspace in Relativity.Workspaces.Where(x => x.ArtifactId == workspaceId).ToArray())
			{
				Relativity.Workspaces.Remove(workspace);
			}
		}
	}
}
