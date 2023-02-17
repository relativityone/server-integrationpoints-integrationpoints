using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class KeplerObjectRepository : IObjectRepository
    {
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly int ArtifactTypeId;

        public KeplerObjectRepository(IRelativityObjectManager relativityObjectManager, int objectTypeId)
        {
            _relativityObjectManager = relativityObjectManager;
            ArtifactTypeId = objectTypeId;
        }

        public Task<ArtifactDTO[]> GetFieldsFromObjects(string[] fields)
        {
            var query = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = ArtifactTypeId },
                Fields = fields.Select(x => new FieldRef { Name = x }),
                SampleParameters = null,
                RelationalField = null,
                SearchProviderCondition = null
            };

            return _relativityObjectManager
                .QueryAsync(query)
                .ToArtifactDTOsArrayAsyncDeprecated();
        }
    }
}
