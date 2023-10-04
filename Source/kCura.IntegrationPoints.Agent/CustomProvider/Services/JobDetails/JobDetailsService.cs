using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
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

        public async Task<CustomProviderJobDetails> GetJobDetailsAsync(int workspaceId, string jobDetails, string correlationID)
        {
            Guid jobHistoryGuid = Guid.Parse(correlationID);

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
                _logger.LogError(ex, "Failed to get Job History ID for Batch Instance GUID: {correlationID}", correlationID);
                throw;
            }

            if (customProviderJobDetails == null || customProviderJobDetails.JobHistoryGuid == Guid.Empty)
            {
                customProviderJobDetails = new CustomProviderJobDetails
                {
                    BatchInstance = customProviderJobDetails?.BatchInstance ?? Guid.Empty,
                    JobHistoryID = jobHistoryId,
                    JobHistoryGuid = jobHistoryGuid
                };
            }

            _logger.LogInformation("Read custom provider job details: {@details}", customProviderJobDetails);

            return customProviderJobDetails;
        }

        public Task UpdateJobDetailsAsync(Job job, CustomProviderJobDetails jobDetails)
        {
            job.JobDetails = _serializer.Serialize(jobDetails);
            _jobService.UpdateJobDetails(job);

            return Task.CompletedTask;
        }
    }
}
