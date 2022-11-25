using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
    internal interface IItemStatusMonitor
    {
        void AddItem(string itemIdentifier, int artifactId);

        void MarkItemAsRead(string itemIdentifier);

        void MarkItemAsFailed(string itemIdentifier);

        void MarkReadSoFarAsSuccessful();

        void MarkReadSoFarAsFailed();

        IEnumerable<int> GetSuccessfulItemArtifactIds();

        IEnumerable<string> GetSuccessfulItemIdentifiers();

        int ProcessedItemsCount { get; }

        int FailedItemsCount { get; }

        int ReadItemsCount { get; }

        int GetArtifactId(string itemIdentifier);
    }
}