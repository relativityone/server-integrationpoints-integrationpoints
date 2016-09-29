﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ArtifactTreeService : IArtifactTreeService
	{
		private readonly IRSAPIClient _client;
		private readonly IArtifactTreeCreator _treeCreator;

		public ArtifactTreeService(IRSAPIClient client, IArtifactTreeCreator treeCreator)
		{
			_client = client;
			_treeCreator = treeCreator;
		}

		public JsTreeItemDTO GetArtifactTree(string artifactTypeName)
		{
			var artifacts = QueryArtifacts(artifactTypeName);
			return _treeCreator.Create(artifacts);
		}

		private List<Artifact> QueryArtifacts(string artifactTypeName)
		{
			var query = new Query {ArtifactTypeName = artifactTypeName};

			var result = _client.Query(_client.APIOptions, query);
			if (!result.Success)
			{
				throw new NotFoundException("Artifact query failed");
			}
			return result.QueryArtifacts;
		}
	}
}