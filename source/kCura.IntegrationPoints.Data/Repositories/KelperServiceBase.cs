using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public abstract class KelperServiceBase
	{
		protected readonly IObjectQueryManagerAdaptor ObjectQueryManagerAdaptor;

		protected KelperServiceBase(IObjectQueryManagerAdaptor objectQueryManagerAdaptor)
		{
			ObjectQueryManagerAdaptor = objectQueryManagerAdaptor;
		}

		protected async Task<ArtifactDTO[]> RetrieveAllArtifactsAsync(Query query)
		{
			List<ArtifactDTO> results = new List<ArtifactDTO>();
			ObjectQueryResultSet resultSet = await ObjectQueryManagerAdaptor.RetrieveAsync(query, String.Empty);
			int count = 0;
			int totalResult = resultSet.Data.TotalResultCount;
			while (count < totalResult)
			{
				string token = resultSet.Data.QueryToken;
				ArtifactDTO[] batchResult = resultSet.GetResultsAsArtifactDto();
				results.AddRange(batchResult);
				count += batchResult.Length;
				resultSet = await ObjectQueryManagerAdaptor.RetrieveAsync(query, token, count + 1);
			}
			return results.ToArray();
		}
	}
}