using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SourceProviderRepository : ISourceProviderRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public SourceProviderRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public SourceProviderDTO Read(int artifactId)
		{
			var query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.SourceProvider),
				Condition = new WholeNumberCondition(new Guid("4A091F69-D750-441C-A4F0-24C990D208AE"), NumericConditionEnum.EqualTo, artifactId),
				Fields = new List<FieldValue>() { new FieldValue(new Guid(SourceProviderFieldGuids.Name)) }
			};

			QueryResultSet<RDO> queryResultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					queryResultSet = rsapiClient.Repositories.RDO.Query(query, 1);
				}
				catch (Exception e)
				{
					throw new Exception($"Unable to retrieve Source Provider: {e.Message}", e);
				}
			}

			Result<RDO> result = queryResultSet.Results.FirstOrDefault();
			if (!queryResultSet.Success || result == null)
			{
				throw new Exception($"Unable to retrieve Source Provider: {queryResultSet.Message}");
			}

			var dto = new SourceProviderDTO()
			{
				ArtifactId = result.Artifact.ArtifactID,
				Name = result.Artifact.Fields[0].ValueAsFixedLengthText
			};

			return dto;
		}
	}
}