extern alias reactive;

using System;
using kCura.ScheduleQueue.Core;
using Disposable = reactive::System.Reactive.Disposables.Disposable;

namespace kCura.IntegrationPoints.Agent.Context
{
	public class JobContextProvider : IJobContextProvider
	{
		private Job _job;

		public bool IsContextStarted => _job != null;

		public IDisposable StartJobContext(Job job)
		{
			if (IsContextStarted)
			{
				throw new InvalidOperationException("Starting new job context before previous context was disposed");
			}

			_job = job;

			return Disposable.Create(() => _job = null);
		}

		public Job Job
		{
			get
			{
				if (_job == null)
				{
					throw new InvalidOperationException("Job is not present because context wasn't initialized");
				}
				return _job;
			}
		}
	}
}
