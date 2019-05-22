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

		public void MarkItemAsRead(string itemIdentifier)
		{
		}

		public void MarkReadSoFarAsSuccessful()
		{
			IEnumerable<ItemStatus> itemsWithUndefinedStatuses = _items.Values.Where(s => s.IsSuccessful == null);
			foreach (var item in itemsWithUndefinedStatuses)
			{
				item.IsSuccessful = true;
			}
		}

		public void MarkReadSoFarAsFailed()
		{
		}

		public void MarkItemAsFailed(string itemIdentifier)
		{
			ItemStatus itemStatus;
			if (_items.TryGetValue(itemIdentifier, out itemStatus))
			{
				itemStatus.IsSuccessful = false;
			}
		}

		public IEnumerable<int> GetSuccessfulItemArtifactIds()
		{
			IEnumerable<ItemStatus> successfulItems = _items.Values.Where(status => status.IsSuccessful == true);
			return successfulItems.Select(status => status.ArtifactId);
		}
	}
}
