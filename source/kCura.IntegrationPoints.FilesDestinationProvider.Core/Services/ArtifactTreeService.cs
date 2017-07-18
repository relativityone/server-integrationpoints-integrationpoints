using System.Linq;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

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
			IOrderedEnumerable<Artifact> artifacts = _artifactService.GetArtifacts(workspaceId, artifactTypeName).OrderBy(x=>x.Name);
			return _treeCreator.Create(artifacts);
		}
	}
}