using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Core.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    public interface IIdFilesBuilder
    {
        Task<List<CustomProviderBatch>> BuildIdFilesAsync(IDataSourceProvider provider, IntegrationPointDto integrationPoint, string directoryPath);
    }
}
