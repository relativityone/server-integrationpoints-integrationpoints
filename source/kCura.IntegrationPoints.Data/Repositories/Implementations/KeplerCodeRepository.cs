using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client;
using Relativity.Services.ObjectQuery;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerCodeRepository : ICodeRepository
	{
		private readonly IObjectQueryManagerAdaptor _objectQueryManagerAdaptor;

		public KeplerCodeRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor)
		{
			_objectQueryManagerAdaptor = objectQueryManagerAdaptor;
			_objectQueryManagerAdaptor.ArtifactTypeId = (int)ArtifactType.Code;
		}

		public async Task<ArtifactDTO[]> RetrieveCodeAsync(string name)
		{
			var query = new Query()
			{
				Condition = $"'Field' == '{name}'",
				Fields = new string[] { "Name"},
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ObjectQueryResultSet resultSet = await _objectQueryManagerAdaptor.RetrieveAsync(query, String.Empty);
			ArtifactDTO[] codes = resultSet.GetResultsAsArtifactDto();
			return codes;
		}
	}
}