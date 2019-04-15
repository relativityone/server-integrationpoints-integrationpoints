using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class TagSavedSearch : ITagSavedSearch
	{
		public async Task<int> CreateTagSavedSearchAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, int savedSearchFolderArtifactId, CancellationToken token)
		{
			await Task.FromException(new NotImplementedException()).ConfigureAwait(false);
			return 0;
		}
	}
}
