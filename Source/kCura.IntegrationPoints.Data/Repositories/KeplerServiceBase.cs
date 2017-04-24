using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public abstract class KeplerServiceBase : MarshalByRefObject
	{
		protected readonly IObjectQueryManagerAdaptor ObjectQueryManagerAdaptor;

		protected KeplerServiceBase(IObjectQueryManagerAdaptor objectQueryManagerAdaptor)
		{
			ObjectQueryManagerAdaptor = objectQueryManagerAdaptor;
		}

		protected async Task<ArtifactDTO[]> RetrieveAllArtifactsAsync(Query query)
		{
			List<ArtifactDTO> results = new List<ArtifactDTO>();
			int count = 0;
			int totalResult = 0;
			string token = String.Empty;

			do
			{
				ObjectQueryResultSet resultSet = await ObjectQueryManagerAdaptor.RetrieveAsync(query, token ?? String.Empty, count + 1).ConfigureAwait(false);
				totalResult = resultSet.Data.TotalResultCount;
				ArtifactDTO[] batchResult = resultSet.GetResultsAsArtifactDto();
				results.AddRange(batchResult);
				count += batchResult.Length;
				token = resultSet.Data.QueryToken;
			} while (count < totalResult);

			return results.ToArray();
		}

		protected string EscapeSingleQuote(string s)
		{
			return Regex.Replace(s, "'", "\\'");
		}
	}
}