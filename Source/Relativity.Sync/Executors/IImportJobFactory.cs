using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal interface IImportJobFactory
	{
		IImportJob CreateImportJob(ISynchronizationConfiguration configuration, IBatch batch);
	}
}