using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ArtifactService : IArtifactService
	{
		private readonly IRSAPIClient _client;
		private readonly IAPILog _logger;

		public ArtifactService(IRSAPIClient client, IHelper helper)
		{
			_client = client;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ArtifactService>();
		}

		public IEnumerable<Artifact> GetArtifacts(int workspaceArtifactId, string artifactTypeName)
		{
			if (workspaceArtifactId != 0)
			{
				_client.APIOptions.WorkspaceID = workspaceArtifactId;
			}

			var query = new Query { ArtifactTypeName = artifactTypeName };

			QueryResult result = _client.Query(_client.APIOptions, query);

			if (!result.Success)
			{
				LogQueryingArtifactError(artifactTypeName);
				throw new NotFoundException("Artifact query failed");
			}

			return result.QueryArtifacts;
		}

		#region Logging

		private void LogQueryingArtifactError(string artifactTypeName)
		{
			_logger.LogError("Artifact query failed for {ArtifactTypeName}.", artifactTypeName);
		}

		#endregion
	}
}