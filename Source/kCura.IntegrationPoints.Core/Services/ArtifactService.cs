using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using Relativity.API;

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
			return GetArtifactsByCondition(workspaceArtifactId, artifactTypeName);
		}

		public Artifact GetArtifact(int workspaceArtifactId, string artifactTypeName, int artifactId)
		{
			return GetArtifactsByCondition(workspaceArtifactId, artifactTypeName,
				new ObjectCondition("ArtifactID", ObjectConditionEnum.EqualTo, artifactId)).Single();
		}

		private IEnumerable<Artifact> GetArtifactsByCondition(int workspaceArtifactId, string artifactTypeName, Condition condition = null)
		{
			if (workspaceArtifactId != 0)
			{
				_client.APIOptions.WorkspaceID = workspaceArtifactId;
			}

			var query = new Query
			{
				ArtifactTypeName = artifactTypeName,
				Condition = condition
			};

			QueryResult result = _client.Query(_client.APIOptions, query);

			if (!result.Success)
			{
				LogQueryingArtifactError(artifactTypeName, result.Message);
				throw new IntegrationPointsException($"Artifact query failed: {result.Message}")
				{
					ExceptionSource = IntegrationPointsExceptionSource.KEPLER
				};
			}

			return result.QueryArtifacts;
		}
		#region Logging

		private void LogQueryingArtifactError(string artifactTypeName, string message)
		{
			_logger.LogError("Artifact query failed for {ArtifactTypeName}. Details: {Message}", artifactTypeName, message);
		}

		#endregion
	}
}