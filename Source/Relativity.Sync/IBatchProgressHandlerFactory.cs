using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal interface IBatchProgressHandlerFactory
	{
		IBatchProgressHandler CreateBatchProgressHandler(IBatch batch, IImportNotifier importNotifier);
	}
}