using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal interface IImportJobFactory
	{
		Task<IImportJob> CreateNativeImportJobAsync(IDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token);

		Task<IImportJob> CreateImageImportJobAsync(IImageSynchronizationConfiguration configuration, IBatch batch, CancellationToken token);

		Task<IImportJob> CreateRdoImportJobAsync(INonDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token);
	}
}