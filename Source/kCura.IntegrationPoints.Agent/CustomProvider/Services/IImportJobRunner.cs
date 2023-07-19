using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal interface IImportJobRunner
    {
        Task<ImportJobResult> RunJobAsync(Job job, CustomProviderJobDetails jobDetails, IntegrationPointInfo integrationPointInfo, ImportJobContext importJobContext, IDataSourceProvider sourceProvider, CompositeCancellationToken token);
    }
}
