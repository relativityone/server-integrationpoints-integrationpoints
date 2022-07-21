using System.Collections.Generic;
using System.Reactive.Concurrency;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
    internal interface IJobProgressHandlerFactory
    {
        IJobProgressHandler CreateJobProgressHandler(IEnumerable<IBatch> alreadyExecutedBatches, IScheduler scheduler = null);
    }
}