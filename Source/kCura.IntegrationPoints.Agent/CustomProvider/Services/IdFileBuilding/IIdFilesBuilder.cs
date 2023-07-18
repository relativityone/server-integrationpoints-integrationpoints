using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.IdFileBuilding
{
    public interface IIdFilesBuilder
    {
        Task<List<CustomProviderBatch>> BuildIdFilesAsync(IDataSourceProvider provider, IntegrationPointInfo integrationPoint, string directoryPath);
    }
}
