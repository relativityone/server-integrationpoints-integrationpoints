using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public abstract class KeplerServiceBase : MarshalByRefObject
	{
		private const int _RETRIEVE_BATCH_SIZE = 1000;
		protected readonly IRelativityObjectManager _relativityObjectManager;

		protected KeplerServiceBase(IRelativityObjectManager relativityObjectManager)
		{
			_relativityObjectManager = relativityObjectManager;
		}

		protected async Task<ArtifactDTO[]> RetrieveAllArtifactsAsync(QueryRequest query, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			var results = new List<ArtifactDTO>();
			int totalCount = 0;

			do
			{
				int retrievedCount = results.Count;
				ResultSet<RelativityObject> resultSet = await _relativityObjectManager
					.QueryAsync(query, retrievedCount, _RETRIEVE_BATCH_SIZE, noFields: false, executionIdentity: executionIdentity)
					.ConfigureAwait(false);

				totalCount = resultSet.TotalCount;
				IEnumerable<ArtifactDTO> batchResult = resultSet.Items.Select(MapRelativityObjectToArtifactDTO);
				results.AddRange(batchResult);
			}
			while (results.Count < totalCount);

			return results.ToArray();
		}

		private static ArtifactDTO MapRelativityObjectToArtifactDTO(RelativityObject relativityObject)
		{
			const int artifactTypeID = 0;
			IEnumerable<ArtifactFieldDTO> fields =
				relativityObject.FieldValues.Select(MapFieldValuePairToArtifactFieldDTO);

			return new ArtifactDTO(
				relativityObject.ArtifactID,
				artifactTypeID,
				relativityObject.Name,
				fields);
		}

		private static ArtifactFieldDTO MapFieldValuePairToArtifactFieldDTO(FieldValuePair fieldValuePair)
		{
			return new ArtifactFieldDTO
			{
				Name = fieldValuePair.Field.Name,
				ArtifactId = fieldValuePair.Field.ArtifactID,
				FieldType = fieldValuePair.Field.FieldType.ToString(),
				Value = fieldValuePair.Value
			};
		}

		protected string EscapeSingleQuote(string s)
		{
			return Regex.Replace(s, "'", "\\'");
		}
	}
}