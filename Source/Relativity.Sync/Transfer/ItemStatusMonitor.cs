using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Transfer
{
	internal class ItemStatusMonitor : IItemStatusMonitor
	{
		private readonly Dictionary<string, ItemStatus> _items = new Dictionary<string, ItemStatus>();

		public void AddItem(string itemIdentifier, int artifactId)
		{
			_items[itemIdentifier] = new ItemStatus(artifactId);
		}

		public void MarkItemAsSuccessful(string itemIdentifier)
		{
			MarkItem(itemIdentifier, true);
		}

		public void MarkItemAsFailed(string itemIdentifier)
		{
			MarkItem(itemIdentifier, false);
		}

		private void MarkItem(string itemIdentifier, bool isSuccessful)
		{
			ItemStatus itemStatus;
			if (_items.TryGetValue(itemIdentifier, out itemStatus))
			{
				itemStatus.IsSuccessful = isSuccessful;
			}
		}

		public IEnumerable<int> GetSuccessfulItemArtifactIds()
		{
			IEnumerable<ItemStatus> successfulItems = _items.Values.Where(status => status.IsSuccessful);
			return successfulItems.Select(status => status.ArtifactId);
		}
	}
}
