﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobStopManager : IJobStopManager
	{
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly Timer _timerThread;
		private readonly IJobService _jobService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly Guid _jobIdentifier;
		private readonly long _jobId;
		private readonly CancellationToken _token;
		private bool _disposed;

		/// <summary>
		/// for testing only.
		/// </summary>
		internal TimerCallback Callback { get; }
		private readonly object _callbackLock = new object();

		public JobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, Guid jobHistoryInstanceId, long jobId)
		{
			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
			_jobIdentifier = jobHistoryInstanceId;
			_jobId = jobId;
			Callback = new TimerCallback(state =>
			{
				lock (_callbackLock)
				{
					try
					{
						Job job = _jobService.GetJob(_jobId);
						if (job != null)
						{
							if (job.StopState.HasFlag(StopState.Stopping))
							{
								JobHistory jobHistory = _jobHistoryService.GetRdo(_jobIdentifier);
								if (jobHistory != null && (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending)
									|| jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryProcessing)))
								{
									jobHistory.JobStatus = JobStatusChoices.JobHistoryStopping;
									jobHistoryService.UpdateRdo(jobHistory);
								}

								_cancellationTokenSource.Cancel();
								_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
							}
							else if(job.StopState.HasFlag(StopState.Unstoppable))
							{
								_timerThread.Change(Timeout.Infinite, Timeout.Infinite);
							}
						}
					}
					catch
					{
						// expect the caller to move on, timerThread will check the status again in the next iteration.
					}
				}
			});
			_cancellationTokenSource = new CancellationTokenSource();
			_token = _cancellationTokenSource.Token;
			_timerThread = new Timer(Callback, null, 0, 500);
		}

		public bool IsStoppingRequested()
		{
			return _token.IsCancellationRequested;
		}

		public void ThrowIfStopRequested()
		{
			// Will throw OperationCancelledException if task is canceled.
			_token.ThrowIfCancellationRequested();	
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				try
				{
					_jobService.UpdateStopState(new List<long> { _jobId }, StopState.Unstoppable);
				}
				catch
				{
					// Do not throw exception, we will need to dispose the rest of the objects.
				}
				_cancellationTokenSource.Dispose();
				_timerThread.Dispose();
				_disposed = true;
			}
		}
	}
}