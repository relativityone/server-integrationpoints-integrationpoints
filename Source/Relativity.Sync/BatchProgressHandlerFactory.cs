using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandlerFactory : IBatchProgressHandlerFactory
	{
		private readonly IBatchProgressUpdater _batchProgressUpdater;
		private readonly IDateTime _dateTime;

		public BatchProgressHandlerFactory(IBatchProgressUpdater batchProgressUpdater, IDateTime dateTime)
		{
			_batchProgressUpdater = batchProgressUpdater;
			_dateTime = dateTime;
		}

		public IBatchProgressHandler CreateBatchProgressHandler(IBatch batch, IImportNotifier importNotifier)
		{
			var handler = new BatchProgressHandler(batch, _batchProgressUpdater, _dateTime);
			importNotifier.OnProcessProgress += handler.HandleProcessProgress;
			importNotifier.OnComplete += handler.HandleProcessComplete;
			return handler;
		}
	}
}