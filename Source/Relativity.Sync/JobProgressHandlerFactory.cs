using Relativity.Sync.Storage;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace Relativity.Sync
{
	internal sealed class JobProgressHandlerFactory : IJobProgressHandlerFactory
	{
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;

		public JobProgressHandlerFactory(IJobProgressUpdaterFactory jobProgressUpdaterFactory)
		{
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
		}

		public IJobProgressHandler CreateJobProgressHandler(IEnumerable<IBatch> alreadyExecutedBatches, IScheduler scheduler = null)
		{
			return new JobProgressHandler(_jobProgressUpdaterFactory.CreateJobProgressUpdater(), alreadyExecutedBatches, scheduler);
		}
	}
}