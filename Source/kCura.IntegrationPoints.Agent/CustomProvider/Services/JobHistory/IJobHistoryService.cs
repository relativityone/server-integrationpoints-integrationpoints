﻿using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory
{
    public interface IJobHistoryService
    {
        Task<Data.JobHistory> ReadJobHistoryByGuidAsync(int workspaceId, Guid jobHistoryGuid);

        Task<int> CreateJobHistoryAsync(int workspaceId, Data.JobHistory jobHistory);

        Task<int> CreateScheduledJobHistoryAsync(int workspaceId, Guid jobHistoryId, IntegrationPointDto integrationPoint);

        Task TryUpdateStartTimeAsync(int workspaceId, int jobHistoryId);

        Task TryUpdateEndTimeAsync(int workspaceId, int integrationPointId, int jobHistoryId);

        Task UpdateStatusAsync(int workspaceId, int integrationPointId, int jobHistoryId, Guid statusGuid);

        Task SetTotalItemsAsync(int workspaceId, int jobHistoryId, int totalItemsCount);

        Task SetJobIdAsync(int workspaceId, int jobHistoryId, string jobID);

        Task UpdateReadItemsCountAsync(int workspaceId, int jobHistoryId, int readItemsCount);

        Task UpdateProgressAsync(int workspaceId, int jobHistoryId, int importedItemsCount, int failedItemsCount);
    }
}
