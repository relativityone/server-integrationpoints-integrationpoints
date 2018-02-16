using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Objects.DataContracts;
using SearchProviderCondition = Relativity.Services.ObjectQuery.SearchProviderCondition;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerObjectRepository : KeplerServiceBase, IObjectRepository
	{
		private int ArtifactTypeId;

		public KeplerObjectRepository(IRelativityObjectManager relativityObjectManager, int objectTypeId)
			: base(relativityObjectManager)
		{
			ArtifactTypeId = objectTypeId;
		}

		public async Task<ArtifactDTO[]> GetFieldsFromObjects(string[] fields)
		{
			var query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = ArtifactTypeId },
				Fields = fields.Select(x => new FieldRef { Name = x }),
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null
			};

			return await RetrieveAllArtifactsAsync(query);
		}
	}
}