using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FieldMapping
{
    public interface IFieldMapService
    {
        Task<IndexedFieldMap> GetIdentifierFieldAsync(int workspaceId, int artifactTypeId, IList<IndexedFieldMap> fieldMap);
    }
}