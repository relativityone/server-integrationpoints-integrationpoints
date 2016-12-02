using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ArtifactTreeService : IArtifactTreeService
	{
		private readonly IArtifactService _artifactService;
		private readonly IArtifactTreeCreator _treeCreator;

		public ArtifactTreeService(IArtifactService artifactService, IArtifactTreeCreator treeCreator)
		{
			_artifactService = artifactService;
			_treeCreator = treeCreator;
		}

		public JsTreeItemDTO GetArtifactTreeWithWorkspaceSet(string artifactTypeName, int workspaceId = 0)
		{
			var artifacts = _artifactService.GetArtifacts(workspaceId, artifactTypeName);

			return _treeCreator.Create(artifacts);
		}
	}
}