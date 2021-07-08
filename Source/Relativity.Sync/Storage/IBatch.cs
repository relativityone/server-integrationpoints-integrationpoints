using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IBatch
	{
		int ArtifactId { get; }
		int StartingIndex { get; }
		BatchStatus Status { get; }
		
		int TransferredItemsCount { get; }
		int FailedItemsCount { get; }

		int TotalDocumentsCount { get; }
		int TransferredDocumentsCount { get; }
		int FailedDocumentsCount { get; }

		int TaggedDocumentsCount { get; }

		Task SetTransferredItemsCountAsync(int transferredItemsCount);
		Task SetFailedItemsCountAsync(int failedItemsCount);

		Task SetTransferredDocumentsCountAsync(int transferredDocumentsCount);
		Task SetFailedDocumentsCountAsync(int failedDocumentsCount);

		Task SetStatusAsync(BatchStatus status);
		Task SetTaggedItemsCountAsync(int taggedDocumentsCount);
		Task SetStartingIndexAsync(int newStartIndex);
	}
}