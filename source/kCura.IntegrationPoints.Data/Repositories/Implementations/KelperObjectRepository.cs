using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KelperObjectRepository : IObjectRepository
	{
		private readonly IObjectQueryManagerAdaptor _objectQueryManagerAdaptor;

		public KelperObjectRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor, int objectTypeId)
		{
			_objectQueryManagerAdaptor = objectQueryManagerAdaptor;
			_objectQueryManagerAdaptor.ArtifactTypeId = objectTypeId;
		}

		public async Task<ArtifactDTO[]> GetFieldsFromObjects(string[] fields)
		{
			var query = new Query()
			{
				Fields = fields,
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ObjectQueryResultSet resultSet = await _objectQueryManagerAdaptor.RetrieveAsync(query, String.Empty);
			ArtifactDTO[] results = resultSet.GetResultsAsArtifactDto();
			return results;
		}
	}
}