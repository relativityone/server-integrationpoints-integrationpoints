using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Core.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding
{
    internal interface ILoadFileBuilder
    {
        Task<DataSourceSettings> CreateDataFileAsync(IStorageAccess<string> storage, CustomProviderBatch batch, IDataSourceProvider provider, IntegrationPointDto integrationPointDto, string importDirectory, List<IndexedFieldMap> fieldMap);
    }
}