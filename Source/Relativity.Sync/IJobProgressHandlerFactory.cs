using System.Reactive.Concurrency;

namespace Relativity.Sync
{
	internal interface IJobProgressHandlerFactory
	{
		IJobProgressHandler CreateJobProgressHandler(IScheduler scheduler = null);
	}
}