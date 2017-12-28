using System;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Context
{
	public class JobContextProvider
	{
		private Job _job;
		private readonly IDisposable _disposableJobContext;

		public JobContextProvider()
		{
			_disposableJobContext = new JobContextDisposable(this);
		}

		public bool IsContextStarted => _job != null;

		public IDisposable StartJobContext(Job job)
		{
			if (IsContextStarted)
			{
				throw new InvalidOperationException("Starting new job context before previous context was disposed");
			}
			_job = job;

			return _disposableJobContext;
		}

		public Job Job
		{
			get
			{
				if (_job == null)
				{
					throw new InvalidOperationException("Job is not present because contex wasn't initialized");
				}
				return _job;
			}
		}

		private class JobContextDisposable : IDisposable
		{
			private JobContextProvider _jobContext;

			public JobContextDisposable(JobContextProvider jobContext)
			{
				_jobContext = jobContext;
			}

			public void Dispose()
			{
				_jobContext._job = null;
			}
		}
	}
}
