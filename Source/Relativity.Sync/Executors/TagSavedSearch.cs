using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal sealed class TagSavedSearch : ITagSavedSearch
	{
		public async Task<int> CreateTagSavedSearchAsync(int workspaceArtifactId, TagsContainer tagsContainer, int savedSearchFolderId, CancellationToken token)
		{
			await Task.FromException(new NotImplementedException()).ConfigureAwait(false);
			return 0;
		}
	}
}
