using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Relativity.Sync
{
	internal sealed class JobProgressHandlerFactory : IJobProgressHandlerFactory
	{
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;

		public JobProgressHandlerFactory(IJobProgressUpdaterFactory jobProgressUpdaterFactory)
		{
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
		}

		public IJobProgressHandler CreateJobProgressHandler(IScheduler scheduler = null)
		{
			return new JobProgressHandler(_jobProgressUpdaterFactory.CreateJobProgressUpdater(), scheduler);
		}
	}
}