using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SourceWorkspaceJobHistoryRepository : ISourceWorkspaceJobHistoryRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public SourceWorkspaceJobHistoryRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public SourceWorkspaceJobHistoryDTO Retrieve(int jobHistoryArtifactId)
		{
			RDO rdo = null;
			try
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

					rdo = rsapiClient.Repositories.RDO.ReadSingle(jobHistoryArtifactId);
				}
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve Job History", e);
			}

			var sourceWorkspaceJobHistoryDto = new SourceWorkspaceJobHistoryDTO()
			{
				ArtifactId = rdo.ArtifactID,
				Name = rdo.TextIdentifier
			};

			return sourceWorkspaceJobHistoryDto;
		}
	}
}