using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class KeplerCodeRepository : ICodeRepository
    {
        private readonly IRelativityObjectManager _relativityObjectManager;

        public KeplerCodeRepository(IRelativityObjectManager relativityObjectManager)
        {
            _relativityObjectManager = relativityObjectManager;
        }

        public Task<ArtifactDTO[]> RetrieveCodeAsync(string name)
        {
            var query = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Code },
                Condition = $"'Field' == '{name.EscapeSingleQuote()}'",
                Fields = new List<FieldRef> { new FieldRef { Name = "Name" } },
                SampleParameters = null,
                RelationalField = null,
                SearchProviderCondition = null,
            };

            return _relativityObjectManager
                .QueryAsync(query)
                .ToArtifactDTOsArrayAsyncDeprecated();
        }
    }
}
