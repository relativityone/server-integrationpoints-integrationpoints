using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal interface IImportJobFactory
	{
		Task<IImportJob> CreateNativeImportJobAsync(ISynchronizationConfiguration configuration, IBatch batch, CancellationToken token);

		Task<IImportJob> CreateImageImportJobAsync(ISynchronizationConfiguration configuration, IBatch batch, CancellationToken token);
	}
}