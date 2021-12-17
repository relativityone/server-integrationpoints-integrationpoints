using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal interface ITaggingProvider
	{
		Task<TaggingExecutionResult> TagDocumentsAsync(IImportJob importJob,
			ISynchronizationConfiguration configuration,
			CompositeCancellationToken token, IDocumentTagRepository documentTagRepository, ISyncLog logger);
	}
}