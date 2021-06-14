using Relativity.Sync.Storage;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace Relativity.Sync
{
	internal interface IJobProgressHandlerFactory
	{
		IJobProgressHandler CreateJobProgressHandler(IEnumerable<IBatch> alreadyExecutedBatches, IScheduler scheduler = null);
	}
}