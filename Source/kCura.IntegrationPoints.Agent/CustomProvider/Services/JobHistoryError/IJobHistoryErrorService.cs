using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError
{
    internal interface IJobHistoryErrorService
    {
        Task AddJobErrorAsync(int workspaceId, int jobHistoryId, Exception ex);
    }
}
