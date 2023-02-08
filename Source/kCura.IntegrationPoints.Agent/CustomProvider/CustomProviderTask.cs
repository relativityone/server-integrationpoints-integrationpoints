using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Interfaces;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class CustomProviderTask : ICustomProviderTask
    {
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ISourceProviderService _sourceProviderService;
        private readonly IRecordIdService _recordIdService;
        private readonly IFileShareService _fileShareService;
        private readonly ISerializer _serializer;
        private readonly IJobService _jobService;
        private readonly IAPILog _logger;

        public CustomProviderTask(IIntegrationPointService integrationPointService, ISourceProviderService sourceProviderService, IRecordIdService recordIdService, IFileShareService fileShareService, ISerializer serializer, IJobService jobService, IAPILog logger)
        {
            _integrationPointService = integrationPointService;
            _sourceProviderService = sourceProviderService;
            _recordIdService = recordIdService;
            _fileShareService = fileShareService;
            _serializer = serializer;
            _jobService = jobService;
            _logger = logger;
        }

        public void Execute(Job job)
        {
            ExecuteAsync(job).GetAwaiter().GetResult();
        }

        private async Task ExecuteAsync(Job job)
        {
            DirectoryInfo importDirectory = null;

            try
            {
                Guid jobId = Guid.NewGuid();

                string fileSharePath = await _fileShareService.GetWorkspaceFileShareLocationAsync(job.WorkspaceID).ConfigureAwait(false);

                if (!Directory.Exists(fileSharePath))
                {
                    throw new DirectoryNotFoundException($"Fileshare directory does not exist: {fileSharePath}");
                }

                importDirectory = new DirectoryInfo(Path.Combine(fileSharePath, "RIP", "CustomProvider-Import", jobId.ToString()));
                importDirectory.Create();

                IntegrationPointDto integrationPointDto = _integrationPointService.Read(job.RelatedObjectArtifactID);

                IDataSourceProvider provider = await _sourceProviderService.GetSourceProviderAsync(job.WorkspaceID, integrationPointDto.SourceProvider);

                List<CustomProviderBatch> batches = await _recordIdService.BuildIdFilesAsync(provider, integrationPointDto, importDirectory.FullName).ConfigureAwait(false);

                CustomProviderJobDetails jobDetails = new CustomProviderJobDetails()
                {
                    JobID = jobId,
                    Batches = batches
                };

                job.JobDetails = _serializer.Serialize(jobDetails);
                _jobService.UpdateJobDetails(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Custom Provider job");
                // TODO REL-806942 Mark job as failed
            }
            finally
            {
                importDirectory?.Delete(true);
            }
        }
    }
}
