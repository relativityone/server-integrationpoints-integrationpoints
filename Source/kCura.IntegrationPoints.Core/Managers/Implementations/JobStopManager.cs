﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class JobStopManager : IJobStopManager
    {
        private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(0.5);
        private readonly bool _supportsDrainStop;
        private readonly CancellationTokenSource _stopCancellationTokenSource;
        private readonly CancellationTokenSource _drainStopCancellationTokenSource;
        private readonly Guid _jobBatchIdentifier;
        private readonly IJobService _jobService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly long _jobId;
        private readonly IRemovableAgent _agent;
        private readonly IAPILog _logger;
        private readonly CancellationToken _token;
        private readonly Timer _timer;
        private bool _isTerminateInProgress;
        private bool _isCleanupJobDrainStopInProgress;
        private bool _isDrainStopping;
        private bool _disposed;

        public event EventHandler<EventArgs> StopRequestedEvent;

        public JobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, IHelper helper,
            Guid jobHistoryInstanceId, long jobId, IRemovableAgent agent, bool supportsDrainStop,
            CancellationTokenSource stopCancellationTokenSource, CancellationTokenSource drainStopCancellationTokenSource)
        {
            _jobService = jobService;
            _jobHistoryService = jobHistoryService;
            _jobBatchIdentifier = jobHistoryInstanceId;
            _jobId = jobId;
            _agent = agent;
            _supportsDrainStop = supportsDrainStop;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<JobStopManager>();

            _stopCancellationTokenSource = stopCancellationTokenSource;
            _drainStopCancellationTokenSource = drainStopCancellationTokenSource;
            _token = _stopCancellationTokenSource.Token;
            _timer = new Timer(state => Execute(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public bool ShouldDrainStop => _isDrainStopping;

        public bool IsStopRequested()
        {
            return _token.IsCancellationRequested;
        }

        public void ThrowIfStopRequested()
        {
            try
            {
                // Will throw OperationCanceledException if task is canceled.
                _token.ThrowIfCancellationRequested();
            }
            catch (Exception)
            {
                _logger.LogWarning("Stop was requested for JobId {jobId}", _jobId);
                throw;
            }
        }

        public void StopCheckingDrainStop()
        {
            _logger.LogInformation("StopCheckingDrainStop was called for Job {jobId}", _jobId);
            _isDrainStopping = false;
        }

        public void CleanUpJobDrainStop()
        {
            if (_isCleanupJobDrainStopInProgress)
            {
                return;
            }

            _logger.LogInformation("CleanUpDrainStop was called for Job {jobId}", _jobId);

            try
            {
                _isCleanupJobDrainStopInProgress = true;
                _jobService.UpdateStopState(new List<long>() { _jobId }, StopState.None);
            }
            finally
            {
                _isCleanupJobDrainStopInProgress = false;
            }
        }

        public void Dispose()
        {
            _logger?.LogInformation("Disposing JobStopManager for Job {jobId}", _jobId);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void ActivateTimer()
        {
            _timer.Change(TimeSpan.Zero, _timerInterval);
        }

        internal void Execute()
        {
            if (_isTerminateInProgress)
            {
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                _isTerminateInProgress = true;
                TerminateIfRequested();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred during checking if job had been stopped.");
            }
            finally
            {
                _isTerminateInProgress = false;
            }

            sw.Stop();
            long diff = sw.ElapsedMilliseconds - (long)_timerInterval.TotalMilliseconds;
            if (diff > 0)
            {
                _logger.LogWarning("JobStopManager.Execute exceeded Timer interval by {milliseconds} ms", diff);
            }
        }

        internal void TerminateIfRequested()
        {
            bool toBeDrainStopped = _supportsDrainStop && _agent.ToBeRemoved && !_isDrainStopping;
            if (toBeDrainStopped)
            {
                _isDrainStopping = true;
            }

            Job job = _jobService.GetJob(_jobId);
            if (job == null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _logger.LogInformation("Job with {id} does not exist. Timer was paused", _jobId);
                return;
            }

            if (toBeDrainStopped)
            {
                _logger.LogInformation("Drain-Stop was requested, retrieving the job - {stopState}", job.StopState);

                if (!job.StopState.HasFlag(StopState.DrainStopping) && !job.StopState.HasFlag(StopState.DrainStopped))
                {
                    _logger.LogInformation("DrainStopping Job {jobId}... JobInfo: {jobInfo}", _jobId, job.ToString());
                    _jobService.UpdateStopState(new List<long>() { _jobId }, StopState.DrainStopping);
                    _drainStopCancellationTokenSource.Cancel();
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            else
            {
                if (job.StopState.HasFlag(StopState.Stopping))
                {
                    _logger.LogInformation("Stopping Job {jobId}... JobInfo: {jobInfo}", _jobId, job.ToString());
                    JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(_jobBatchIdentifier);

                    if (jobHistory == null)
                    {
                        _logger.LogWarning("JobHistory of id: {batchInstance} not found");
                    }
                    else if (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending) || jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryProcessing))
                    {
                        _logger.LogInformation("Set JobHistory to Stopping {jobHistory}", jobHistory.Stringify());
                        SetJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryStopping);
                    }

                    _stopCancellationTokenSource.Cancel();
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    RaiseStopRequestedEvent();
                }
                else if (job.StopState.HasFlag(StopState.Unstoppable))
                {
                    _logger.LogInformation("Job is unstoppable, disabling JobStopManager timer");
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _stopCancellationTokenSource.Dispose();
                _drainStopCancellationTokenSource.Dispose();
                _timer.Dispose();
                _disposed = true;
            }
        }

        protected virtual void RaiseStopRequestedEvent()
        {
            StopRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void SetJobHistoryStatus(JobHistory jobHistory, ChoiceRef status)
        {
            jobHistory.JobStatus = status;
            _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
        }
    }
}
