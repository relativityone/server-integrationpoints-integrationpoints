using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Transfer
{
    internal class ItemStatusMonitor : IItemStatusMonitor
    {
        private readonly Dictionary<string, ItemInfo> _items = new Dictionary<string, ItemInfo>();

        public void AddItem(string itemIdentifier, int artifactId)
        {
            _items[itemIdentifier] = new ItemInfo(artifactId);
        }

        public void MarkItemAsRead(string itemIdentifier)
        {
            MarkItemWithStatus(itemIdentifier, ItemStatus.Read);
        }

        public void MarkItemAsFailed(string itemIdentifier)
        {
            MarkItemWithStatus(itemIdentifier, ItemStatus.Failed);
        }

        private void MarkItemWithStatus(string itemIdentifier, ItemStatus status)
        {
            ItemInfo item;
            if (_items.TryGetValue(itemIdentifier, out item))
            {
                item.Status = status;
            }
        }

        public void MarkReadSoFarAsSuccessful()
        {
            MarkReadSoFarWithStatus(ItemStatus.Succeed);
        }

        public void MarkReadSoFarAsFailed()
        {
            MarkReadSoFarWithStatus(ItemStatus.Failed);
        }

        private void MarkReadSoFarWithStatus(ItemStatus itemStatus)
        {
            IEnumerable<ItemInfo> itemsRead = _items.Values.Where(s => s.Status == ItemStatus.Read);
            foreach (var item in itemsRead)
            {
                item.Status = itemStatus;
            }
        }

        public IEnumerable<int> GetSuccessfulItemArtifactIds()
        {
            IEnumerable<ItemInfo> successfulItems = _items.Values.Where(item => item.Status == ItemStatus.Succeed);
            IEnumerable<int> artifactIds = successfulItems.Select(status => status.ArtifactId);
            return artifactIds;
        }

        public IEnumerable<string> GetSuccessfulItemIdentifiers()
        {
            IEnumerable<KeyValuePair<string, ItemInfo>> successfulItems = _items.Where(item => item.Value.Status == ItemStatus.Succeed);
            IEnumerable<string> identifiers = successfulItems.Select(item => item.Key);
            return identifiers;
        }

        public int ProcessedItemsCount => _items.Count(x => x.Value.Status == ItemStatus.Succeed);

        public int FailedItemsCount => _items.Count(x => x.Value.Status == ItemStatus.Failed);

        public int GetArtifactId(string itemIdentifier)
        {
            if (_items.ContainsKey(itemIdentifier))
            {
                return _items[itemIdentifier].ArtifactId;
            }

            return -1;
        }

        private class ItemInfo
        {
            public int ArtifactId { get; }

            public ItemStatus Status { get; set; }

            public ItemInfo(int artifactId)
            {
                ArtifactId = artifactId;
                Status = ItemStatus.Created;
            }
        }

        private enum ItemStatus
        {
            Created = 0,
            Read,
            Succeed,
            Failed
        }
    }
}
