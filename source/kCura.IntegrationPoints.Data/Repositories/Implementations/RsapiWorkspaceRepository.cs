using System;
using System.Collections.Generic;
using System.Linq;
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
			QueryResultSet<Workspace> resultSet = null;
			try
			{
				WholeNumberCondition workspaceCondition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.EqualTo, workspaceArtifactId);
				Query<Workspace> query = new Query<Workspace>
				{
					Condition = workspaceCondition,
					Fields = new List<FieldValue>() { new FieldValue() { Name = "Name" } }
				};

				resultSet = _rsapiClient.Repositories.Workspace.Query(query);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve Workspace", e);
			}

			RdoHelper.CheckResult(resultSet);
			Workspace workspace = resultSet.Results.First().Artifact;

			var workspaceDto = new WorkspaceDTO()
			{
				ArtifactId = workspace.ArtifactID,
				Name = workspace.Name
			};

			return workspaceDto;
		}
	}
}