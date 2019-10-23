using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Relativity.Sync
{
	internal sealed class JobProgressHandlerFactory : IJobProgressHandlerFactory
	{
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly IDateTime _dateTime;

		public JobProgressHandlerFactory(IJobProgressUpdaterFactory jobProgressUpdaterFactory, IDateTime dateTime)
		{
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
			_dateTime = dateTime;
		}

		public IJobProgressHandler CreateJobProgressHandler(IScheduler scheduler = null)
		{
			return new JobProgressHandler(_jobProgressUpdaterFactory.CreateJobProgressUpdater(), scheduler);
		}
	}
}