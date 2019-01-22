using System;
using System.Threading;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobStopManager : IJobStopManager
	{
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly Guid _jobBatchIdentifier;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly long _jobId;
		private readonly IJobService _jobService;
		private readonly IAPILog _logger;
		private readonly Timer _timerThread;
		private readonly CancellationToken _token;
		private bool _disposed;

		/// <summary>
		///     for testing only.
		/// </summary>
		internal TimerCallback Callback { get; }

		public object SyncRoot { get; }

		public JobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, IHelper helper, Guid jobHistoryInstanceId, long jobId, CancellationTokenSource cancellationTokenSource)
		{
			SyncRoot = new object();
			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
			_jobBatchIdentifier = jobHistoryInstanceId;
			_jobId = jobId;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<JobStopManager>();

			Callback = state =>
			{
				lock (SyncRoot)
				{
					try
					{
						var job = _jobService.GetJob(_jobId);
						if (job != null)
						{
							if (job.StopState.HasFlag(StopState.Stopping))
							{
								var jobHistory = _jobHistoryService.GetRdo(_jobBatchIdentifier);
								if ((jobHistory != null) && (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending)
															|| jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryProcessing)))
								{
									jobHistory.JobStatus = JobStatusChoices.JobHistoryStopping;
									jobHistoryService.UpdateRdo(jobHistory);
								}

								_cancellationTokenSource.Cancel();
								_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
								LogStoppingJob();
								RaiseStopRequestedEvent();
							}
							else if (job.StopState.HasFlag(StopState.Unstoppable))
							{
								_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
							}
						}
					}
					catch (Exception e)
					{
						LogErrorDuringStopCheck(e);
						// expect the caller to move on, timerThread will check the status again in the next iteration.
					}
				}
			};
			_cancellationTokenSource = cancellationTokenSource;
			_token = _cancellationTokenSource.Token;
			_timerThread = new Timer(Callback, null, 0, 500);
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
				_cancellationTokenSource.Dispose();
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