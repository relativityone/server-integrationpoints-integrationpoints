using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding
{
    internal interface ILoadFileBuilder
    {
        Task<DataSourceSettings> CreateDataFileAsync(CustomProviderBatch batch, IDataSourceProvider provider, IntegrationPointInfo integrationPointInfo, string importDirectory);
    }
}