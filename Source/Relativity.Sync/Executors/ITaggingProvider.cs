using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal interface ITaggingProvider
	{
		Task<TaggingExecutionResult> TagObjectsAsync(IImportJob importJob,
			ISynchronizationConfiguration configuration,
			CompositeCancellationToken token);
	}
}