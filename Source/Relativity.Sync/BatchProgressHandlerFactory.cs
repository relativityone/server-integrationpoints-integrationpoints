using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandlerFactory : IBatchProgressHandlerFactory
	{
		private readonly ISyncLog _logger;
		private readonly IDateTime _dateTime;

		public BatchProgressHandlerFactory(ISyncLog logger, IDateTime dateTime)
		{
			_logger = logger;
			_dateTime = dateTime;
		}

		public IBatchProgressHandler CreateBatchProgressHandler(IBatch batch, IImportNotifier importNotifier)
		{
			return new BatchProgressHandler(importNotifier, new BatchProgressUpdater(batch, _logger), _dateTime);
		}
	}
}