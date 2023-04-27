using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Data;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    internal interface IJobProgressHandler
    {
        Task<IDisposable> BeginUpdateAsync(ImportJobContext importJobContext);

        Task SafeUpdateProgressAsync(ImportJobContext importJobContext);

        Task WaitForJobToFinish(ImportJobContext importJobContext, CompositeCancellationToken token);

        Task UpdateReadItemsCountAsync(Job job, CustomProviderJobDetails jobDetails);

        Task SetTotalItemsAsync(int workspaceId, int jobHistoryId, int totalItemsCount);
    }
}
