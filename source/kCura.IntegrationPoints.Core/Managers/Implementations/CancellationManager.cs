using System;
using System.Threading;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class CancellationManager : ICancellationManager
	{
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly Timer _timerThread;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly JobHistory _jobHistory;
		private readonly Job _job;
		private bool _disposed;

		internal TimerCallback Callback { get; }

		public CancellationManager(IRepositoryFactory repositoryFactory, JobHistory jobHistory, Job job)
		{
			_repositoryFactory = repositoryFactory;
			_jobHistory = jobHistory;
			_job = job;
			Callback = new TimerCallback(state =>
			{
				try
				{
					if (_job != null)
					{
						// TODO : check if the job history's status is in canceling. SAMO - 7/26
						// if (false)
						// {
						//	_cancellationTokenSource.Cancel();
						// }
					}
				}
				catch
				{
					// expect the caller to move on, timerThread will check the status again in the next iteration.
				}
			});
			_cancellationTokenSource = new CancellationTokenSource();
			_timerThread = new Timer(Callback, null, 0, 500);
		}


		public bool IsCancellationRequested()
		{
			// TODO : update the job history's status to canceling, if the job is marked as canceling. SAMO - 7/26
			return _cancellationTokenSource.IsCancellationRequested;
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
				_cancellationTokenSource.Dispose();
				_timerThread.Dispose();
				_disposed = true;
			}
		}
	}
}