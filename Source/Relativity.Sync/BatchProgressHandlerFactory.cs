using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandlerFactory : IBatchProgressHandlerFactory
	{
		public IBatchProgressHandler CreateBatchProgressHandler(IBatch batch, IImportNotifier importNotifier)
		{
			return new BatchProgressHandler(batch, importNotifier);
		}
	}
}