using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal interface IImportJobFactory
	{
		Task<IImportJob> CreateImportJobAsync(ISynchronizationConfiguration configuration, IBatch batch);
	}
}