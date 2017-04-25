using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerCodeRepository : KeplerServiceBase, ICodeRepository
	{
		public KeplerCodeRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor)
			: base(objectQueryManagerAdaptor)
		{
			ObjectQueryManagerAdaptor.ArtifactTypeId = (int)ArtifactType.Code;
		}

		public async Task<ArtifactDTO[]> RetrieveCodeAsync(string name)
		{
			var query = new Query()
			{
				Condition = $"'Field' == '{EscapeSingleQuote(name)}'",
				Fields = new string[] { "Name" },
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