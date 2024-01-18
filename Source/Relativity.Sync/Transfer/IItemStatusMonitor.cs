using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
    internal interface IItemStatusMonitor
    {
        int ProcessedItemsCount { get; }

        int FailedItemsCount { get; }

        int ReadItemsCount { get; }

        void AddItem(string itemIdentifier, int artifactId);

        void MarkItemAsRead(string itemIdentifier);

        void MarkItemAsFailed(string itemIdentifier);

        void MarkReadSoFarAsSuccessful();

        void MarkReadSoFarAsFailed();

        IEnumerable<int> GetSuccessfulItemArtifactIds();

        IEnumerable<string> GetSuccessfulItemIdentifiers();

        int GetArtifactId(string itemIdentifier);
    }
}
