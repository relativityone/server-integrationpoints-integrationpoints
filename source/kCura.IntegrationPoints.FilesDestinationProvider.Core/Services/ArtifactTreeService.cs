using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ArtifactTreeService : IArtifactTreeService
	{
		private readonly IRSAPIClient _client;
		private readonly IArtifactTreeCreator _treeCreator;
		private readonly IAPILog _logger;

		public ArtifactTreeService(IRSAPIClient client, IArtifactTreeCreator treeCreator, IHelper helper)
		{
			_client = client;
			_treeCreator = treeCreator;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ArtifactTreeService>();
		}

		public JsTreeItemDTO GetArtifactTreeWithWorkspaceSet(string artifactTypeName, int workspaceId = 0)
		{
			SetQueryWorkspaceIdIfNotDefault(workspaceId);
			return GetArtifactTree(artifactTypeName);
		}

		private JsTreeItemDTO GetArtifactTree(string artifactTypeName)
		{

			List<Artifact> artifacts = QueryArtifacts(artifactTypeName);
			return _treeCreator.Create(artifacts);
		}

		private List<Artifact> QueryArtifacts(string artifactTypeName)
		{
			var query = new Query { ArtifactTypeName = artifactTypeName };

			QueryResult result = _client.Query(_client.APIOptions, query);
			if (!result.Success)
			{
				LogQueryingArtifactError(artifactTypeName);
				throw new NotFoundException("Artifact query failed");
			}
			return result.QueryArtifacts;
		}

		private void SetQueryWorkspaceIdIfNotDefault(int workspaceId)
		{
			if (workspaceId != 0)
				_client.APIOptions.WorkspaceID = workspaceId;
		}

		#region Logging

		private void LogQueryingArtifactError(string artifactTypeName)
		{
			_logger.LogError("Artifact query failed for {ArtifactTypeName}.", artifactTypeName);
		}

		#endregion
	}
}