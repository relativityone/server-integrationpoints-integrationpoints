﻿using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    internal interface IJobProgressHandler
    {
        Task<IDisposable> BeginUpdateAsync(ImportJobContext importJobContext);
        Task UpdateProgressAsync(ImportJobContext importJobContext);
        Task WaitForJobToFinish(ImportJobContext importJobContext);
        Task UpdateReadItemsCountAsync(Job job, CustomProviderJobDetails jobDetails);
        Task SetTotalItemsAsync(int workspaceId, int jobHistoryId, int totalItemsCount);
    }
}