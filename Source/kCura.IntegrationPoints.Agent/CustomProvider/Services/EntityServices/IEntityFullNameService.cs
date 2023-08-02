using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices
{
    public interface IEntityFullNameService
    {
        Task<IntegrationPointInfo> HandleFullNameMappingIfNeededAsync(IntegrationPointInfo integrationPoint);

        string FormatFullName(Dictionary<string, IndexedFieldMap> destinationFieldNameToFieldMapDictionary, IDataReader reader);
    }
}