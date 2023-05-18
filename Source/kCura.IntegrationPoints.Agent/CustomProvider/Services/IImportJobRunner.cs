using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal interface IImportJobRunner
    {
        Task RunJobAsync(Job job, CustomProviderJobDetails jobDetails, IntegrationPointDto integrationPointDto, IDataSourceProvider sourceProvider, CompositeCancellationToken token);
    }
}
