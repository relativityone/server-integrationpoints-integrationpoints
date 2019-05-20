using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
	internal interface IItemStatusMonitor
	{
		void AddItem(string itemIdentifier, int artifactId);
		void MarkItemAsFailed(string itemIdentifier);
		IEnumerable<int> GetSuccessfulItemArtifactIds();
		void MarkItemAsSuccessful(string itemIdentifier);
	}
}