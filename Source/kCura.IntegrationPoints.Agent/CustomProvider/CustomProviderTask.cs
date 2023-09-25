using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
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
using Newtonsoft.Json;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Services.Choice;
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
        private readonly IEntityFullNameService _entityFullNameService;
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
            IEntityFullNameService entityFullNameService,
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
            _entityFullNameService = entityFullNameService;
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
            _logger.LogInformation("Running custom provider JobID: {jobId}.", job.JobId);

            CustomProviderJobDetails jobDetails = await _jobDetailsService.GetJobDetailsAsync(job.WorkspaceID, job.JobDetails, job.CorrelationID).ConfigureAwait(false);
            ImportJobContext importJobContext = new ImportJobContext(job.WorkspaceID, job.JobId, Guid.Parse(job.CorrelationID), jobDetails.JobHistoryID);
            IntegrationPointDto integrationPointDto = null;

            await _jobHistoryService.TryUpdateStartTimeAsync(job.WorkspaceID, jobDetails.JobHistoryID).ConfigureAwait(false);

            try
            {
                integrationPointDto = _integrationPointService.Read(job.RelatedObjectArtifactID);

                LogIntegrationPointConfiguration(integrationPointDto);

                IntegrationPointInfo integrationPointInfo = new IntegrationPointInfo(integrationPointDto);

                await ValidateJobAsync(job, jobDetails.JobHistoryID, integrationPointDto).ConfigureAwait(false);

                IDataSourceProvider sourceProvider = await _sourceProviderService.GetSourceProviderAsync(job.WorkspaceID, integrationPointDto.SourceProvider).ConfigureAwait(false);

                CompositeCancellationToken token = _cancellationTokenFactory.GetCancellationToken(jobDetails.JobHistoryGuid, job.JobId);

                await ConfigureBatchesAsync(job, integrationPointInfo, jobDetails, sourceProvider).ConfigureAwait(false);

                ImportJobResult endResult = await _importJobRunner
                    .RunJobAsync(job, jobDetails, integrationPointInfo, importJobContext, sourceProvider, token)
                    .ConfigureAwait(false);

                await ReportJobEndAsync(job, endResult, jobDetails).ConfigureAwait(false);
            }
            catch (IntegrationPointValidationException e)
            {
                await HandleExceptionAsync(job.WorkspaceID, job.RelatedObjectArtifactID, jobDetails.JobHistoryID, JobStatusChoices.JobHistoryValidationFailedGuid, e).ConfigureAwait(false);
                throw;
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(job.WorkspaceID, job.RelatedObjectArtifactID, jobDetails.JobHistoryID, JobStatusChoices.JobHistoryErrorJobFailedGuid, e).ConfigureAwait(false);
                throw;
            }
            finally
            {
                if (integrationPointDto != null)
                {
                    await _notificationService.PrepareAndSendEmailNotificationAsync(importJobContext, integrationPointDto.EmailNotificationRecipients).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogWarning("Integration Point object is null. Email notification will not be send.");
                }

                await _jobHistoryService.TryUpdateEndTimeAsync(job.WorkspaceID, job.RelatedObjectArtifactID, jobDetails.JobHistoryID).ConfigureAwait(false);
            }
        }

        private async Task ValidateJobAsync(Job job, int jobHistoryId, IntegrationPointDto integrationPoint)
        {
            await _jobHistoryService.UpdateStatusAsync(job.WorkspaceID, integrationPoint.ArtifactId, jobHistoryId, JobStatusChoices.JobHistoryValidatingGuid).ConfigureAwait(false);

            _agentValidator.Validate(integrationPoint, job.SubmittedBy);
        }

        private async Task ConfigureBatchesAsync(Job job, IntegrationPointInfo integrationPoint, CustomProviderJobDetails jobDetails, IDataSourceProvider sourceProvider)
        {
            if (!jobDetails.Batches.Any())
            {
                DirectoryInfo importDirectory = await _relativityStorageService.PrepareImportDirectoryAsync(job.WorkspaceID, jobDetails.JobHistoryGuid);

                jobDetails.Batches = await _idFilesBuilder.BuildIdFilesAsync(sourceProvider, integrationPoint, importDirectory.FullName).ConfigureAwait(false);
                await _jobDetailsService.UpdateJobDetailsAsync(job, jobDetails).ConfigureAwait(false);
            }

            int totalItemsCount = jobDetails.Batches.Sum(x => x.NumberOfRecords);
            await _jobHistoryService.SetTotalItemsAsync(job.WorkspaceID, jobDetails.JobHistoryID, totalItemsCount).ConfigureAwait(false);

            await _jobHistoryService.UpdateStatusAsync(job.WorkspaceID, integrationPoint.ArtifactId, jobDetails.JobHistoryID, JobStatusChoices.JobHistoryProcessingGuid).ConfigureAwait(false);
        }

        private async Task ReportJobEndAsync(Job job, ImportJobResult endResult, CustomProviderJobDetails details)
        {
            ChoiceRef jobHistoryStatus;
            switch (endResult.Status)
            {
                case JobEndStatus.Completed:
                    jobHistoryStatus = details.Batches.Any(x => x.Status == BatchStatus.CompletedWithErrors)
                        ? JobStatusChoices.JobHistoryCompletedWithErrors
                        : JobStatusChoices.JobHistoryCompleted;
                    break;
                case JobEndStatus.Failed:
                    jobHistoryStatus = JobStatusChoices.JobHistoryErrorJobFailed;
                    HandleImportServiceFailure(endResult);
                    break;
                case JobEndStatus.Canceled:
                    jobHistoryStatus = JobStatusChoices.JobHistoryStopped;
                    break;
                case JobEndStatus.DrainStopped:
                    jobHistoryStatus = JobStatusChoices.JobHistorySuspending;
                    break;
                default:
                    throw new NotSupportedException($"Unknown Import Job State - {endResult.Status}");
            }

            await _jobHistoryService.UpdateStatusAsync(job.WorkspaceID, job.RelatedObjectArtifactID, details.JobHistoryID, jobHistoryStatus.Guids[0]).ConfigureAwait(false);

            _logger.LogInformation("Custom Provider job finished - {status}, ImportDetails: {@result}", jobHistoryStatus.Name, endResult);
        }

        private void HandleImportServiceFailure(ImportJobResult endResult)
        {
            if (endResult.Status == JobEndStatus.Failed)
            {
                string messages = endResult.Errors != null && endResult.Errors.Any() ? string.Join(Environment.NewLine, endResult.Errors) : "Import API transfer failed";
                throw new Exception($"Job failed with following errors: {messages}");
            }
        }

        private async Task HandleExceptionAsync(int workspaceId, int integrationPointId, int jobHistoryId, Guid status, Exception e)
        {
            _logger.LogError(e, "Failed to execute Custom Provider job.");

            await _jobHistoryService.UpdateStatusAsync(workspaceId, integrationPointId, jobHistoryId, status).ConfigureAwait(false);

            await _jobHistoryErrorService.AddJobErrorAsync(workspaceId, jobHistoryId, e).ConfigureAwait(false);
        }

        private void LogIntegrationPointConfiguration(IntegrationPointDto integrationPointDto)
        {
            try
            {
                _logger
                    .ForContext("SourceConfiguration", JsonConvert.DeserializeObject(integrationPointDto.SourceConfiguration), true)
                    .ForContext("DestinationConfiguration", integrationPointDto.DestinationConfiguration, true)
                    .LogInformation("Read IntegrationPoint Configuration {artifactId}.", integrationPointDto.ArtifactId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred when logging Integration Point configuration");
            }
        }
    }
}
