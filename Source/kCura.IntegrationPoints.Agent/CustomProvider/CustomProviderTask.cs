using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IdFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobCancellation;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.SourceProvider;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Sync;
using BatchStatus = kCura.IntegrationPoints.Agent.CustomProvider.DTO.BatchStatus;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class CustomProviderTask : ICustomProviderTask
    {
        private readonly ICancellationTokenFactory _cancellationTokenFactory;
        private readonly IAgentValidator _agentValidator;
        private readonly IJobDetailsService _jobDetailsService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ISourceProviderService _sourceProviderService;
        private readonly IImportJobRunner _importJobRunner;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IIdFilesBuilder _idFilesBuilder;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly INotificationService _notificationService;
        private readonly IAPILog _logger;

        public CustomProviderTask(
            ICancellationTokenFactory cancellationTokenFactory,
            IAgentValidator agentValidator,
            IJobDetailsService jobDetailsService,
            IIntegrationPointService integrationPointService,
            ISourceProviderService sourceProviderService,
            IImportJobRunner importJobRunner,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IIdFilesBuilder idFilesBuilder,
            IRelativityStorageService relativityStorageService,
            INotificationService notificationService,
            IAPILog logger)
        {
            _cancellationTokenFactory = cancellationTokenFactory;
            _agentValidator = agentValidator;
            _jobDetailsService = jobDetailsService;
            _integrationPointService = integrationPointService;
            _sourceProviderService = sourceProviderService;
            _importJobRunner = importJobRunner;
            _jobHistoryService = jobHistoryService;
            _jobHistoryErrorService = jobHistoryErrorService;
            _idFilesBuilder = idFilesBuilder;
            _relativityStorageService = relativityStorageService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public void Execute(Job job)
        {
            ExecuteAsync(job).GetAwaiter().GetResult();
        }

        private async Task ExecuteAsync(Job job)
        {
            CustomProviderJobDetails jobDetails = await _jobDetailsService.GetJobDetailsAsync(job.WorkspaceID, job.JobDetails).ConfigureAwait(false);
            IntegrationPointDto integrationPointDto = null;
            try
            {
                integrationPointDto = _integrationPointService.Read(job.RelatedObjectArtifactID);

                await ValidateJobAsync(job, jobDetails.JobHistoryID, integrationPointDto).ConfigureAwait(false);

                IDataSourceProvider sourceProvider = await _sourceProviderService.GetSourceProviderAsync(job.WorkspaceID, integrationPointDto.SourceProvider);

                CompositeCancellationToken token = _cancellationTokenFactory.GetCancellationToken(jobDetails.JobHistoryGuid, job.JobId);

                await ConfigureBatchesAsync(job, integrationPointDto, jobDetails, sourceProvider).ConfigureAwait(false);

                ImportJobResult endResult = await _importJobRunner
                    .RunJobAsync(job, jobDetails, integrationPointDto, sourceProvider, token)
                    .ConfigureAwait(false);

                await ReportJobEndAsync(job, endResult, jobDetails).ConfigureAwait(false);
            }
            catch (IntegrationPointValidationException e)
            {
                await HandleExceptionAsync(job.WorkspaceID, jobDetails.JobHistoryID, JobStatusChoices.JobHistoryValidationFailedGuid, e).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(job.WorkspaceID, jobDetails.JobHistoryID, JobStatusChoices.JobHistoryErrorJobFailedGuid, e).ConfigureAwait(false);
            }
            finally
            {
                if (integrationPointDto != null)
                {
                    var importJobContext = new ImportJobContext(job.WorkspaceID, job.JobId, jobDetails.JobHistoryGuid, jobDetails.JobHistoryID);
                    await _notificationService.PrepareAndSendEmailNotificationAsync(importJobContext, integrationPointDto.EmailNotificationRecipients).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogWarning("Integration Point object is null. Email notification will not be send.");
                }
            }
        }

        private async Task ReportJobEndAsync(Job job, ImportJobResult endResult, CustomProviderJobDetails details)
        {
            Guid jobHistoryStatus;
            switch (endResult.Status)
            {
                case JobEndStatus.Completed:
                    jobHistoryStatus = details.Batches.Any(x => x.Status == BatchStatus.CompletedWithErrors)
                        ? JobStatusChoices.JobHistoryCompletedWithErrorsGuid
                        : JobStatusChoices.JobHistoryCompletedGuid;
                    break;
                case JobEndStatus.Failed:
                    jobHistoryStatus = JobStatusChoices.JobHistoryErrorJobFailedGuid;
                    break;
                case JobEndStatus.Canceled:
                    jobHistoryStatus = JobStatusChoices.JobHistoryStoppedGuid;
                    break;
                case JobEndStatus.DrainStopped:
                    jobHistoryStatus = JobStatusChoices.JobHistorySuspendingGuid;
                    break;
                default:
                    throw new NotSupportedException($"Unknown Import Job State - {endResult.Status}");
            }

            await _jobHistoryService.UpdateStatusAsync(job.WorkspaceID, details.JobHistoryID, jobHistoryStatus).ConfigureAwait(false);
        }

        private async Task ValidateJobAsync(Job job, int jobHistoryId, IntegrationPointDto integrationPoint)
        {
            await _jobHistoryService.UpdateStatusAsync(job.WorkspaceID, jobHistoryId, JobStatusChoices.JobHistoryValidatingGuid).ConfigureAwait(false);

            _agentValidator.Validate(integrationPoint, job.SubmittedBy);
        }

        private async Task ConfigureBatchesAsync(Job job, IntegrationPointDto integrationPoint, CustomProviderJobDetails jobDetails, IDataSourceProvider sourceProvider)
        {
            if (!jobDetails.Batches.Any())
            {
                DirectoryInfo importDirectory = await _relativityStorageService.PrepareImportDirectoryAsync(job.WorkspaceID, jobDetails.JobHistoryGuid);

                jobDetails.Batches = await _idFilesBuilder.BuildIdFilesAsync(sourceProvider, integrationPoint, importDirectory.FullName).ConfigureAwait(false);
                await _jobDetailsService.UpdateJobDetailsAsync(job, jobDetails).ConfigureAwait(false);
            }

            int totalItemsCount = jobDetails.Batches.Sum(x => x.NumberOfRecords);
            await _jobHistoryService.SetTotalItemsAsync(job.WorkspaceID, jobDetails.JobHistoryID, totalItemsCount).ConfigureAwait(false);

            await _jobHistoryService.UpdateStatusAsync(job.WorkspaceID, jobDetails.JobHistoryID, JobStatusChoices.JobHistoryProcessingGuid).ConfigureAwait(false);
        }

        private async Task HandleExceptionAsync(int workspaceId, int jobHistoryId, Guid status, Exception e)
        {
            _logger.LogError(e, "Failed to execute Custom Provider job.");

            await _jobHistoryService.UpdateStatusAsync(workspaceId, jobHistoryId, status).ConfigureAwait(false);

            await _jobHistoryErrorService.AddJobErrorAsync(workspaceId, jobHistoryId, e).ConfigureAwait(false);
        }
    }
}