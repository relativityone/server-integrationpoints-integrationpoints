using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerObjectRepository : KeplerServiceBase, IObjectRepository
	{
		public KeplerObjectRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor, int objectTypeId)
			: base(objectQueryManagerAdaptor)
		{
			ObjectQueryManagerAdaptor.ArtifactTypeId = objectTypeId;
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

			return await RetrieveAllArtifactsAsync(query);
		}
	}
}