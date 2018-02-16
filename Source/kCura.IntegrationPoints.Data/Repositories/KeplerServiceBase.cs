using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.ObjectQuery;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public abstract class KeplerServiceBase : MarshalByRefObject
	{
		protected readonly IRelativityObjectManager _relativityObjectManager;

		protected KeplerServiceBase(IRelativityObjectManager relativityObjectManager)
		{
			_relativityObjectManager = relativityObjectManager;
		}

		protected async Task<ArtifactDTO[]> RetrieveAllArtifactsAsync(QueryRequest query)
		{
			List<ArtifactDTO> results = new List<ArtifactDTO>();
			int count = 0;
			int totalResult = 0;
			int batchSize = 1000;

			do
			{
				var resultSet = await _relativityObjectManager.QueryAsync(query, count, batchSize).ConfigureAwait(false);
				totalResult = resultSet.TotalCount;
				ArtifactDTO[] batchResult = resultSet.Items.Select(x => new ArtifactDTO(x.ArtifactID, 0, x.Name,
					x.FieldValues.Select(y =>
						new ArtifactFieldDTO()
						{
							Name = y.Field.Name,
							ArtifactId = y.Field.ArtifactID,
							FieldType = y.Field.FieldType.ToString(),
							Value = y.Value
						}))).ToArray();
				results.AddRange(batchResult);
				count += batchResult.Length;
			} while (count < totalResult);

			return results.ToArray();
		}

		protected string EscapeSingleQuote(string s)
		{
			return Regex.Replace(s, "'", "\\'");
		}
	}
}