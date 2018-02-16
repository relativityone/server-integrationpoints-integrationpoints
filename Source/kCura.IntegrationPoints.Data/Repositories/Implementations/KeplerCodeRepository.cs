using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerCodeRepository : KeplerServiceBase, ICodeRepository
	{
		public KeplerCodeRepository(IRelativityObjectManager relativityObjectManager)
			: base(relativityObjectManager)
		{

		}

		public async Task<ArtifactDTO[]> RetrieveCodeAsync(string name)
		{
			var query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Code },
				Condition = $"'Field' == '{EscapeSingleQuote(name)}'",
				Fields = new List<FieldRef>{ new FieldRef {Name = "Name" } },
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
			};

			return await RetrieveAllArtifactsAsync(query);
		}
	}
}