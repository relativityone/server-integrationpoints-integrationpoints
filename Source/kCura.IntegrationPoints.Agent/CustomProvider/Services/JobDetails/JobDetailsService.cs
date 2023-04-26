using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails
{
    public class JobDetailsService : IJobDetailsService
    {
        private readonly IJobService _jobService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly ISerializer _serializer;
        private readonly IAPILog _logger;

        public JobDetailsService(IJobService jobService, IJobHistoryService jobHistoryService, ISerializer serializer, IAPILog logger)
        {
            _jobService = jobService;
            _jobHistoryService = jobHistoryService;
            _serializer = serializer;
            _logger = logger;
        }

        public async Task<CustomProviderJobDetails> GetJobDetailsAsync(int workspaceId, string jobDetails)
        {
            Guid jobHistoryGuid = GetBatchInstance(jobDetails);

            CustomProviderJobDetails customProviderJobDetails = null;

            try
            {
                customProviderJobDetails = _serializer.Deserialize<CustomProviderJobDetails>(jobDetails ?? string.Empty);
            }
            catch (RipSerializationException ex)
            {
                _logger.LogWarning(ex, $"Unexpected content inside job-details: {jobDetails}", ex.Value);
            }

            int jobHistoryId;

            try
            {
                Data.JobHistory jobHistory = await _jobHistoryService.ReadJobHistoryByGuidAsync(workspaceId, jobHistoryGuid).ConfigureAwait(false);
                jobHistoryId = jobHistory.ArtifactId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Job History ID for Batch Instance GUID: {batchInstance}", jobHistoryGuid);
                throw;
            }

            if (customProviderJobDetails == null || customProviderJobDetails.JobHistoryGuid == Guid.Empty)
            {
                customProviderJobDetails = new CustomProviderJobDetails()
                {
                    JobHistoryID = jobHistoryId,
                    JobHistoryGuid = jobHistoryGuid
                };
            }

            _logger.LogInformation("Running custom provider job ID: {importJobId}", customProviderJobDetails.JobHistoryGuid);

            return customProviderJobDetails;
        }

        public Task UpdateJobDetailsAsync(Job job, CustomProviderJobDetails jobDetails)
        {
            job.JobDetails = _serializer.Serialize(jobDetails);
            _jobService.UpdateJobDetails(job);

            return Task.CompletedTask;
        }

        private Guid GetBatchInstance(string jobDetails)
        {
            try
            {
                Guid? deserializedBatchInstance = _serializer.Deserialize<TaskParameters>(jobDetails)?.BatchInstance;

                if (deserializedBatchInstance.HasValue && deserializedBatchInstance.Value != Guid.Empty)
                {
                    return deserializedBatchInstance.Value;
                }
                else
                {
                    return GetJobHistoryGuid(jobDetails);
                }
            }
            catch
            {
                return GetJobHistoryGuid(jobDetails);
            }
        }

        private Guid GetJobHistoryGuid(string jobDetails)
        {
            CustomProviderJobDetails customProviderJobDetails = _serializer.Deserialize<CustomProviderJobDetails>(jobDetails ?? string.Empty);
            return customProviderJobDetails.JobHistoryGuid;
        }
    }
}
