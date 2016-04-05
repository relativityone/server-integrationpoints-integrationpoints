using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiWorkspaceRepository : IWorkspaceRepository
	{
		private readonly IRSAPIClient _rsapiClient;

		public RsapiWorkspaceRepository(IRSAPIClient rsapiClient)
		{
			_rsapiClient = rsapiClient;
		}

		public WorkspaceDTO Retrieve(int workspaceArtifactId)
		{
			_rsapiClient.APIOptions.WorkspaceID = -1;

			Workspace workspaceRdo = null;
			try
			{
				workspaceRdo = _rsapiClient.Repositories.Workspace.ReadSingle(workspaceArtifactId);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve Workspace", e);
			}

			var workspaceDto = new WorkspaceDTO()
			{
				ArtifactId = workspaceRdo.ArtifactID,
				Name = workspaceRdo.Name
			};

			return workspaceDto;
		}
	}
}