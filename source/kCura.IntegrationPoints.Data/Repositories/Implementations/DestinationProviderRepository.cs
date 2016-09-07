using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class DestinationProviderRepository : IDestinationProviderRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public DestinationProviderRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public DestinationProviderDTO Read(int artifactId)
		{
			var query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.DestinationProvider),
				Condition = new WholeNumberCondition(new Guid(Domain.Constants.DESTINATION_PROVIDER_ARTIFACTID_FIELD), NumericConditionEnum.EqualTo, artifactId),
				Fields = new List<FieldValue>
				{
					new FieldValue(new Guid(DestinationProviderFieldGuids.Identifier)),
					new FieldValue(new Guid(DestinationProviderFieldGuids.Name))
				}
			};

			using (IRSAPIClient client = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				client.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					QueryResultSet<RDO> resultSet = client.Repositories.RDO.Query(query, 1);
					if (!resultSet.Success)
					{
						throw new Exception(resultSet.Message);
					}

					Result<RDO> result = resultSet.Results.First();

					var provider = new DestinationProviderDTO
					{
						ArtifactId = result.Artifact.ArtifactID,
						Identifier = new Guid(result.Artifact.Fields[0].ValueAsFixedLengthText),
						Name = result.Artifact.Fields[1].ValueAsFixedLengthText
					};

					return provider;
				}
				catch (Exception ex)
				{
					throw new Exception($"Unable to retrieve Destination Provider: {ex.Message}", ex);
				}
			}
		}
	}
}