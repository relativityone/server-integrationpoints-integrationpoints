using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandlerFactory : IBatchProgressHandlerFactory
	{
		private readonly ISyncLog _logger;

		public BatchProgressHandlerFactory(ISyncLog logger)
		{
			_logger = logger;
		}

		public IBatchProgressHandler CreateBatchProgressHandler(IBatch batch, IImportNotifier importNotifier)
		{
			return new BatchProgressHandler(importNotifier, new BatchProgressUpdater(batch, _logger));
		}
	}
}