using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using Relativity;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FieldMapping
{
    public class FieldMapService : IFieldMapService
    {
        private readonly IEntityFullNameObjectManagerService _entityFullNameObjectManagerService;

        public FieldMapService(IEntityFullNameObjectManagerService entityFullNameObjectManagerService)
        {
            _entityFullNameObjectManagerService = entityFullNameObjectManagerService;
        }

        public async Task<IndexedFieldMap> GetIdentifierFieldAsync(int workspaceId, int artifactTypeId, IList<IndexedFieldMap> fieldMap)
        {
            if (artifactTypeId == (int)ArtifactType.Document) // avoid calling IEntityFullNameObjectManagerService unnecessarily
            {
                return fieldMap.FirstOrDefault(x => x.FieldMap.DestinationField.IsIdentifier);
            }
            else
            {
                bool isEntity = await _entityFullNameObjectManagerService.IsEntityAsync(workspaceId, artifactTypeId).ConfigureAwait(false);

                if (isEntity)
                {
                    return fieldMap.FirstOrDefault(x => x.DestinationFieldName == EntityFieldNames.FullName);
                }
                else
                {
                    return fieldMap.FirstOrDefault(x => x.FieldMap.DestinationField.IsIdentifier);
                }
            }
        }
    }
}