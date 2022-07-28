using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banzai;
using Relativity.API;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync
{
    internal sealed class SyncJob : ISyncJob
    {
        private readonly INode<SyncExecutionContext> _pipeline;
        private readonly ISyncExecutionContextFactory _executionContextFactory;
        private readonly SyncJobParameters _syncJobParameters;
        private readonly IProgress<SyncJobState> _syncProgress;
        private readonly ISyncToggles _syncToggles;
        private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
        private readonly IAPILog _logger;

        public SyncJob(INode<SyncExecutionContext> pipeline, ISyncExecutionContextFactory executionContextFactory, SyncJobParameters syncJobParameters, IProgress<SyncJobState> syncProgress,
            ISyncToggles syncToggles, IJobProgressUpdaterFactory jobProgressUpdaterFactory, IAPILog logger)
        {
            _pipeline = pipeline;
            _executionContextFactory = executionContextFactory;
            _syncJobParameters = syncJobParameters;
            _syncProgress = syncProgress;
            _syncToggles = syncToggles;
            _jobProgressUpdaterFactory = jobProgressUpdaterFactory;
            _logger = logger;
        }

        public Task ExecuteAsync(CompositeCancellationToken token)
        {
            return ExecuteAsync(token, _syncProgress);
        }

        public Task ExecuteAsync(IProgress<SyncJobState> progress, CompositeCancellationToken token)
        {
            IProgress<SyncJobState> safeProgress = new SafeProgressWrapper<SyncJobState>(progress, _logger);
            return ExecuteAsync(token, _syncProgress, safeProgress);
        }

        private async Task ExecuteAsync(CompositeCancellationToken token, params IProgress<SyncJobState>[] progressReporters)
        {
            if (_syncToggles.IsEnabled<EnableJobHistoryStatusUpdate>())
            {
                _logger.LogInformation("Job History status will be updated");

                IJobProgressUpdater jobProgressUpdater = _jobProgressUpdaterFactory.CreateJobProgressUpdater();

                token.DrainStopCancellationToken.Register(() => jobProgressUpdater.UpdateJobStatusAsync(JobHistoryStatus.Suspending).GetAwaiter().GetResult());

                try
                {
                    await jobProgressUpdater.SetJobStartedAsync().ConfigureAwait(false);

                    IProgress<SyncJobState> progress = new Progress<SyncJobState>(async syncJobState => await jobProgressUpdater.UpdateJobStatusAsync(GetJobHistoryStatus(syncJobState.Id)).ConfigureAwait(false));
                    List<IProgress<SyncJobState>> aggregatedProgress = new List<IProgress<SyncJobState>>
                    {
                        progress
                    };
                    aggregatedProgress.AddRange(progressReporters);

                    await InternalExecuteAsync(token, aggregatedProgress.ToArray()).ConfigureAwait(false);

                    if (token.StopCancellationToken.IsCancellationRequested)
                    {
                        await jobProgressUpdater.UpdateJobStatusAsync(JobHistoryStatus.Stopped).ConfigureAwait(false);
                    }
                    else if (token.DrainStopCancellationToken.IsCancellationRequested)
                    {
                        await jobProgressUpdater.UpdateJobStatusAsync(JobHistoryStatus.Suspended).ConfigureAwait(false);
                    }
                    else
                    {
                        await jobProgressUpdater.UpdateJobStatusAsync(JobHistoryStatus.Completed).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    await jobProgressUpdater.UpdateJobStatusAsync(JobHistoryStatus.Stopped).ConfigureAwait(false);
                }
                catch (ValidationException ex)
                {
                    await jobProgressUpdater.UpdateJobStatusAsync(JobHistoryStatus.ValidationFailed).ConfigureAwait(false);
                    await jobProgressUpdater.AddJobErrorAsync(ex).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await jobProgressUpdater.UpdateJobStatusAsync(JobHistoryStatus.Failed).ConfigureAwait(false);
                    await jobProgressUpdater.AddJobErrorAsync(ex).ConfigureAwait(false);
                }
            }
            else
            {
                await InternalExecuteAsync(token, progressReporters).ConfigureAwait(false);
            }
        }

        private JobHistoryStatus GetJobHistoryStatus(string syncStatus)
        {
            const string validating = "validating";
            const string checkingPermissions = "checking permissions";

            if (syncStatus.Equals(validating, StringComparison.InvariantCultureIgnoreCase) || syncStatus.Equals(checkingPermissions, StringComparison.InvariantCultureIgnoreCase))
            {
                return JobHistoryStatus.Validating;
            }
            else
            {
                return JobHistoryStatus.Processing;
            }
        }

        private async Task InternalExecuteAsync(CompositeCancellationToken token, params IProgress<SyncJobState>[] progressReporters)
        {
            NodeResult executionResult;
            try
            {
                IProgress<SyncJobState> combinedProgress = progressReporters.Combine();
                IExecutionContext<SyncExecutionContext> executionContext = _executionContextFactory.Create(combinedProgress, token);
                executionResult = await _pipeline.ExecuteAsync(executionContext).ConfigureAwait(false);
                _logger.LogInformation("Sync job completed with execution result: {result}", executionResult.Status);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogWarning(e, "Operation cancelled.");
                throw;
            }
            catch (SyncException e)
            {
                _logger.LogError(e, "SyncException has been thrown during job execution.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured during Sync job execution.");
                throw new SyncException("Error occured during Sync job execution. See inner exception for more details.", e, _syncJobParameters.WorkflowId);
            }

            if (executionResult.Status != NodeResultStatus.Succeeded && executionResult.Status != NodeResultStatus.SucceededWithErrors)
            {
                SyncExecutionContext subject = (SyncExecutionContext)executionResult.Subject;
                IList<Exception> failingExceptions = subject.Results
                    .Where(r => r.Exception != null)
                    .Select(r => r.Exception)
                    .ToList();

                ValidationException validationException = failingExceptions.OfType<ValidationException>().FirstOrDefault();
                if (validationException != null)
                {
                    _logger.LogWarning(validationException, "Sync job validation failed.");
                    throw new ValidationException(validationException.Message, new AggregateException(failingExceptions), validationException.ValidationResult);
                }

                const string errorMessage = "Sync job failed. See inner exceptions for more details.";
                _logger.LogError(errorMessage);
                throw new SyncException(errorMessage, new AggregateException(failingExceptions), _syncJobParameters.WorkflowId);
            }
        }
    }
}
