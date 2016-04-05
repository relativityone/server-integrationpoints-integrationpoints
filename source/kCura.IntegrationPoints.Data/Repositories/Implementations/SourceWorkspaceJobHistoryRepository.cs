using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SourceWorkspaceJobHistoryRepository : ISourceWorkspaceJobHistoryRepository
	{
		private readonly IRSAPIClient _rsapiClient;

		public SourceWorkspaceJobHistoryRepository(IRSAPIClient rsapiClient)
		{
			_rsapiClient = rsapiClient;
		}

		public SourceWorkspaceJobHistoryDTO Retrieve(int workspaceArtifactId, int jobHistoryArtifactId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			RDO rdo = null;
			try
			{
				rdo = _rsapiClient.Repositories.RDO.ReadSingle(jobHistoryArtifactId);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve Job History", e);
			}

			var sourceWorkspaceJobHistoryDto = new SourceWorkspaceJobHistoryDTO()
			{
				ArtifactId = rdo.ArtifactID,
				Name = rdo.TextIdentifier // TODO: verify this
			};

			return sourceWorkspaceJobHistoryDto;
		}
	}
}