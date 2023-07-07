using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError
{
    internal interface IJobHistoryErrorService
    {
        Task AddJobErrorAsync(int workspaceId, int jobHistoryId, Exception ex);

        Task CreateItemLevelErrorsAsync(int workspaceId, int jobHistoryId, IList<ItemLevelError> errors);

        Task<Data.JobHistoryError> GetLastJobLevelError(int workspaceId, int jobHistoryId);
    }
}
