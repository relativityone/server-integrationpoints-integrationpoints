using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.RelativitySync.Metrics;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Executors.Validation;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.RelativitySync
{
#pragma warning disable CA1031
    public sealed class RelativitySyncAdapter
    {
        private readonly IExtendedJob _job;
        private readonly IAPILog _logger;
        private readonly IAPM _apmMetrics;
        private readonly ISyncJobMetric _jobMetric;
        private readonly IJobHistorySyncService _jobHistorySyncService;
        private readonly Guid _correlationId;
        private readonly IIntegrationPointToSyncConverter _converter;
        private readonly ISyncOperationsWrapper _syncOperations;
        private readonly ISyncConfigurationService _syncConfigurationService;
        private readonly ICancellationAdapter _cancellationAdapter;

        public RelativitySyncAdapter(IExtendedJob job, IAPILog logger, IAPM apmMetrics,
            ISyncJobMetric jobMetric, IJobHistorySyncService jobHistorySyncService, IIntegrationPointToSyncConverter converter,
            ISyncOperationsWrapper syncOperations, ISyncConfigurationService syncConfigurationService, ICancellationAdapter cancellationAdapter)
        {
            _job = job;
            _logger = logger;
            _apmMetrics = apmMetrics;
            _jobMetric = jobMetric;
            _jobHistorySyncService = jobHistorySyncService;
            _converter = converter;
            _syncOperations = syncOperations;
            _syncConfigurationService = syncConfigurationService;
            _cancellationAdapter = cancellationAdapter;

            _correlationId = Guid.NewGuid();
        }

        public async Task<TaskResult> RunAsync()
        {
            TaskResult taskResult = new TaskResult { Status = TaskStatusEnum.Fail };
            SyncMetrics metrics = new SyncMetrics(_apmMetrics, _logger);
            try
            {
                CompositeCancellationToken compositeCancellationToken = _cancellationAdapter.GetCancellationToken(drainStopTokenCallback: MarkJobAsSuspending);
                metrics.MarkStartTime();
                await MarkJobAsStartedAsync().ConfigureAwait(false);

                ISyncJob syncJob = await CreateSyncJobAsync().ConfigureAwait(false);
                IProgress<SyncJobState> progress = new Progress<SyncJobState>(syncJobState => UpdateJobStatusAsync(syncJobState.Id).ConfigureAwait(false).GetAwaiter().GetResult());
                await syncJob.ExecuteAsync(progress, compositeCancellationToken).ConfigureAwait(false);

                if (compositeCancellationToken.StopCancellationToken.IsCancellationRequested)
                {
                    await MarkJobAsStoppedAsync().ConfigureAwait(false);
                    taskResult = new TaskResult { Status = TaskStatusEnum.Success };
                }
                else if (compositeCancellationToken.DrainStopCancellationToken.IsCancellationRequested)
                {
                    await MarkJobAsSuspendedAsync().ConfigureAwait(false);
                    taskResult = new TaskResult { Status = TaskStatusEnum.DrainStopped };
                }
                else
                {
                    await MarkJobAsCompletedAsync().ConfigureAwait(false);
                    taskResult = new TaskResult { Status = TaskStatusEnum.Success };
                }
            }
            catch (OperationCanceledException)
            {
                await MarkJobAsStoppedAsync().ConfigureAwait(false);
                taskResult = new TaskResult { Status = TaskStatusEnum.Fail };
            }
            catch (ValidationException ex)
            {
                await MarkJobAsValidationFailedAsync(ex).ConfigureAwait(false);
                taskResult = new TaskResult() { Status = TaskStatusEnum.Fail };
            }
            catch (Exception e)
            {
                await MarkJobAsFailedAsync(e).ConfigureAwait(false);
                taskResult = new TaskResult { Status = TaskStatusEnum.Fail };
            }
            finally
            {
                metrics.SendMetric(_correlationId, taskResult);
            }

            return taskResult;
        }

        private async Task MarkJobAsValidationFailedAsync(ValidationException ex)
        {
            try
            {
                await _jobHistorySyncService.MarkJobAsValidationFailedAsync(_job, ex).ConfigureAwait(false);
                await _jobMetric.SendJobFailedAsync(_job.Job, ex).ConfigureAwait(false);
            }
            catch (SyncMetricException e)
            {
                _logger.LogError(e, "Failed to send job failed metric");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to mark job as validation failed.");
            }
        }

        private async Task UpdateJobStatusAsync(string status)
        {
            try
            {
                await _jobHistorySyncService.UpdateJobStatusAsync(status, _job).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update job status.");
            }
        }

        private async Task MarkJobAsStartedAsync()
        {
            try
            {
                await _jobHistorySyncService.MarkJobAsStartedAsync(_job).ConfigureAwait(false);
                await _jobMetric.SendJobStartedAsync(_job.Job).ConfigureAwait(false);
            }
            catch (SyncMetricException e)
            {
                _logger.LogError(e, "Failed to send job started metric");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to mark job as started.");
            }
        }

        private async Task MarkJobAsCompletedAsync()
        {
            try
            {
                await _jobHistorySyncService.MarkJobAsCompletedAsync(_job).ConfigureAwait(false);
                await _jobMetric.SendJobCompletedAsync(_job.Job).ConfigureAwait(false);
            }
            catch (SyncMetricException e)
            {
                _logger.LogError(e, "Failed to send job completed metric");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to mark job as completed.");
            }
        }

        private async Task MarkJobAsStoppedAsync()
        {
            try
            {
                await _jobHistorySyncService.MarkJobAsStoppedAsync(_job).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to mark job as stopped.");
            }
        }

        private void MarkJobAsSuspending()
        {
            try
            {
                _jobHistorySyncService.MarkJobAsSuspendingAsync(_job).Wait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to mark job as suspending.");
            }
        }

        private async Task MarkJobAsSuspendedAsync()
        {
            try
            {
                await _jobHistorySyncService.MarkJobAsSuspendedAsync(_job).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to mark job as suspended.");
            }
        }

        private async Task MarkJobAsFailedAsync(Exception exception)
        {
            try
            {
                await _jobHistorySyncService.MarkJobAsFailedAsync(_job, exception).ConfigureAwait(false);
                await _jobMetric.SendJobFailedAsync(_job.Job, exception).ConfigureAwait(false);
            }
            catch (SyncMetricException e)
            {
                _logger.LogError(e, "Failed to send job failed metric");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to mark job as failed.");
            }
        }

        private async Task<ISyncJob> CreateSyncJobAsync()
        {
            int syncConfigurationArtifactId = await PrepareSyncConfigurationAsync().ConfigureAwait(false);

            ISyncJobFactory jobFactory = _syncOperations.CreateSyncJobFactory();
            IRelativityServices relativityServices = _syncOperations.CreateRelativityServices();

            SyncJobParameters parameters = new SyncJobParameters(syncConfigurationArtifactId, _job.WorkspaceId, _job.SubmittedById, _job.JobIdentifier)
            {
                TriggerValue = "rip"
            };

            return jobFactory.Create(parameters, relativityServices, _logger);
        }

        private async Task<int> PrepareSyncConfigurationAsync()
        {
            try
            {
                int? syncConfigurationId = await _syncConfigurationService.TryGetResumedSyncConfigurationIdAsync(
                    _job.WorkspaceId, _job.JobHistoryId).ConfigureAwait(false);

                if (syncConfigurationId.HasValue)
                {
                    _logger.LogInformation("SyncConfiguration with ID {configurationId} exists for JobHistory {jobHistory}. Job is resumed.",
                        syncConfigurationId.Value, _job.JobHistoryId);
                    await _syncOperations.PrepareSyncConfigurationForResumeAsync(
                        _job.WorkspaceId, syncConfigurationId.Value).ConfigureAwait(false);

                    return syncConfigurationId.Value;
                }

                return await _converter.CreateSyncConfigurationAsync(_job).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Sync Configuration RDO preparation failed.");
                throw;
            }
        }
    }
#pragma warning restore CA1031
}
