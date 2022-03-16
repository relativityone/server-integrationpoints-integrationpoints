using System;
using System.Collections.Generic;
using System.Threading;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class JobStopManager : IJobStopManager
	{
		private bool _supportsDrainStop;
		private bool _isDrainStopping;
		private Timer _timerThread;
		private bool _disposed;

		private const int _TIMER_INTERVAL_MS = 500;

		private readonly CancellationTokenSource _stopCancellationTokenSource;
		private readonly CancellationTokenSource _drainStopCancellationTokenSource;
		private readonly Guid _jobBatchIdentifier;
		private readonly IJobService _jobService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly long _jobId;
		private readonly IRemovableAgent _agent;
        private readonly IAPILog _logger;
		private readonly CancellationToken _token;

		public object SyncRoot { get; }

		public JobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, IHelper helper,
			Guid jobHistoryInstanceId, long jobId, IRemovableAgent agent, bool supportsDrainStop,
			CancellationTokenSource stopCancellationTokenSource, CancellationTokenSource drainStopCancellationTokenSource)
		{
			SyncRoot = new object();

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
			_timerThread = new Timer(state => Execute(), null, Timeout.Infinite, Timeout.Infinite);
		}

		internal void ActivateTimer()
		{
			_timerThread.Change(0, _TIMER_INTERVAL_MS);
		}

		internal void Execute()
		{
			lock (SyncRoot)
			{
				try
				{
					Job job = _jobService.GetJob(_jobId);

					if (job != null)
					{
						TerminateIfRequested(job);
					}
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Error occurred during checking if job had been stopped.");
				}
			}
		}

		internal void TerminateIfRequested(Job job)
		{
			if (_supportsDrainStop && _agent.ToBeRemoved)
			{
				_logger.LogInformation("Drain-Stop was requested: SupportsDrainStop - {supportsDrainStop}, AgentToBeRemoved {agentToBeRemoved}",
					_supportsDrainStop, _agent.ToBeRemoved);

				_isDrainStopping = true;

				if (!job.StopState.HasFlag(StopState.DrainStopping) && !job.StopState.HasFlag(StopState.DrainStopped))
				{
					_logger.LogInformation("DrainStopping Job {jobId}... JobInfo: {jobInfo}", _jobId, job.ToString());
					UpdateStopState(StopState.DrainStopping);
					_drainStopCancellationTokenSource.Cancel();
					_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
				}
			}
			else if (job.StopState.HasFlag(StopState.Stopping))
			{
				_logger.LogInformation("Stopping Job {jobId}... JobInfo: {jobInfo}", _jobId, job.ToString());
				JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(_jobBatchIdentifier);

				if ((jobHistory != null) && (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending)
											 || jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryProcessing)))
				{
					_logger.LogInformation("Set JobHistory to Stopping {jobHistory}", jobHistory.Stringify());
					SetJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryStopping);
				}

				_stopCancellationTokenSource.Cancel();
				_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
				RaiseStopRequestedEvent();
			}
			else if (job.StopState.HasFlag(StopState.Unstoppable))
			{
				_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
			}
		}
		
		private void UpdateStopState(StopState stopState)
		{
			_jobService.UpdateStopState(new List<long>() { _jobId }, stopState);
		}

		private void SetJobHistoryStatus(JobHistory jobHistory, ChoiceRef status)
		{
			jobHistory.JobStatus = status;
			_jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
		}

		public bool IsStopRequested()
		{
			return _token.IsCancellationRequested;
		}

		public void ThrowIfStopRequested()
		{
			// Will throw OperationCanceledException if task is canceled.
			_token.ThrowIfCancellationRequested();
		}

		public bool ShouldDrainStop => _isDrainStopping;

		public event EventHandler<EventArgs> StopRequestedEvent;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_stopCancellationTokenSource.Dispose();
				_drainStopCancellationTokenSource.Dispose();
				_timerThread.Dispose();
				_disposed = true;
			}
		}

		protected virtual void RaiseStopRequestedEvent()
		{
			StopRequestedEvent?.Invoke(this, EventArgs.Empty);
		}

        public void StopCheckingDrainStop()
		{
			_logger.LogInformation("StopCheckingDrainStop was called for Job {jobId}", _jobId);
			_supportsDrainStop = false;
			_isDrainStopping = false;
        }

        public void CleanUpJobDrainStop()
        {
			_logger.LogInformation("CleanUpDrainStop was called for Job {jobId}", _jobId);
            UpdateStopState(StopState.None);
        }
	}
}