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

		long MetadataBytesTransferred { get; }
		long FilesBytesTransferred { get; }
		long TotalBytesTransferred { get; }

		int TaggedDocumentsCount { get; }

		Task SetTransferredItemsCountAsync(int transferredItemsCount);
		Task SetFailedItemsCountAsync(int failedItemsCount);

		Task SetTransferredDocumentsCountAsync(int transferredDocumentsCount);
		Task SetFailedDocumentsCountAsync(int failedDocumentsCount);

		Task SetMetadataBytesTransferredAsync(long metadataBytesTransferred);
		Task SetFilesBytesTransferredAsync(long filesBytesTransferred);
		Task SetTotalBytesTransferredAsync(long totalBytesTransferred);

		Task SetStatusAsync(BatchStatus status);
		Task SetTaggedDocumentsCountAsync(int taggedDocumentsCount);
		Task SetStartingIndexAsync(int newStartIndex);
	}
}