using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class ImageSynchronizationExecutionConstrains : BaseSynchronizationExecutionConstrains<IImageSynchronizationConfiguration>
	{
		public ImageSynchronizationExecutionConstrains(IBatchRepository batchRepository, ISyncLog syncLog) : base(batchRepository, syncLog)
		{
		}
	}
}