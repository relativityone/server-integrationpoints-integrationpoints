using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices
{
    public interface IEntityFullNameService
    {
        Task<bool> ShouldHandleFullNameAsync(CustomProviderDestinationConfiguration configuration, List<IndexedFieldMap> fieldsMap);

        Task EnrichFieldMapWithFullNameAsync(IntegrationPointInfo integrationPoint);

        string FormatFullName(Dictionary<string, IndexedFieldMap> destinationFieldNameToFieldMapDictionary, IDataReader reader);
    }
}
