using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface ITagSavedSearch
	{
		Task<int> CreateTagSavedSearchAsync(int workspaceArtifactId, TagsContainer tagsContainer, int savedSearchFolderId, CancellationToken token);
	}
}