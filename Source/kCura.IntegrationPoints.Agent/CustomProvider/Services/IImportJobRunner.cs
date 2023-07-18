using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal interface IImportJobRunner
    {
        Task<ImportJobResult> RunJobAsync(Job job, CustomProviderJobDetails jobDetails, IntegrationPointInfo integrationPointDto, IDataSourceProvider sourceProvider, CompositeCancellationToken token);
    }
}
