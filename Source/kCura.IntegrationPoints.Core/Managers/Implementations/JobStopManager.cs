using System;
using System.Collections.Generic;
using System.Threading;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobStopManager : IJobStopManager
	{
		private const int _TIMER_INTERVAL_MS = 500;

		private readonly CancellationTokenSource _stopCancellationTokenSource;
		private readonly CancellationTokenSource _drainStopCancellationTokenSource;
		private readonly Guid _jobBatchIdentifier;
		private readonly IJobService _jobService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobServiceDataProvider _jobServiceDataProvider;
		private readonly long _jobId;
		private readonly IRemovableAgent _agent;
		private readonly bool _supportsDrainStop;
		private readonly IAPILog _logger;
		private readonly CancellationToken _token;

		private bool _isDrainStopping;
		private Timer _timerThread;
		private bool _disposed;

		public object SyncRoot { get; }

		public JobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, IJobServiceDataProvider jobServiceDataProvider, IHelper helper,
			Guid jobHistoryInstanceId, long jobId, IRemovableAgent agent, bool supportsDrainStop,
			CancellationTokenSource stopCancellationTokenSource, CancellationTokenSource drainStopCancellationTokenSource)
		{
			SyncRoot = new object();

			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
			_jobServiceDataProvider = jobServiceDataProvider;
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
					LogErrorDuringStopCheck(e);
					// expect the caller to move on, timerThread will check the status again in the next iteration.
				}
			}
		}

		internal void TerminateIfRequested(Job job)
		{
			if (_supportsDrainStop && _agent.ToBeRemoved)
			{
				_isDrainStopping = true;
				JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(_jobBatchIdentifier);

				if (!job.StopState.HasFlag(StopState.DrainStopping) && !job.StopState.HasFlag(StopState.DrainStopped)
				                                                    && jobHistory.JobStatus != JobStatusChoices.JobHistorySuspended)
				{
					_drainStopCancellationTokenSource.Cancel();
					UpdateStopState(StopState.DrainStopping);
					SetJobHistoryStatus(jobHistory, JobStatusChoices.JobHistorySuspending);
				}
				else if (jobHistory != null && jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistorySuspended))
				{
					UpdateStopState(StopState.DrainStopped);
					_jobServiceDataProvider.UnlockJob(job.JobId);
					_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
				}
			}
			else if (job.StopState.HasFlag(StopState.Stopping))
			{
				JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(_jobBatchIdentifier);

				if ((jobHistory != null) && (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending)
											 || jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryProcessing)))
				{
					SetJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryStopping);
				}

				_stopCancellationTokenSource.Cancel();
				_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
				LogStoppingJob();
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

		#region Logging

		private void LogErrorDuringStopCheck(Exception e)
		{
			_logger.LogError(e, "Error occurred during checking if job had been stopped.");
		}

		private void LogStoppingJob()
		{
			_logger.LogInformation("Stopping job requested received in agent. Status has been updated.");
		}

		#endregion
	}
}