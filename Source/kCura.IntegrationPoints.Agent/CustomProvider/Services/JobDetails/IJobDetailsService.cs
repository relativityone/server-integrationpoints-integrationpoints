﻿using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails
{
    public interface IJobDetailsService
    {
        Task<CustomProviderJobDetails> GetJobDetailsAsync(int workspaceId, string jobDetails, string correlationID);

        Task UpdateJobDetailsAsync(Job job, CustomProviderJobDetails jobDetails);
    }
}
