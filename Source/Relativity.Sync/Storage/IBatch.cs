using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IBatch
	{
		int ArtifactId { get; }
		int FailedItemsCount { get; }
		string LockedBy { get; }
		double Progress { get; }
		int StartingIndex { get; }
		BatchStatus Status { get; }
		int TransferredItemsCount { get; }
		int TaggedItemsCount { get; }
		long MetadataBytesTransferred { get; }
		long FilesBytesTransferred { get; }
		long TotalBytesTransferred { get; }
		int TotalItemsCount { get; }

		Task SetFailedItemsCountAsync(int failedItemsCount);
		Task SetLockedByAsync(string lockedBy);
		Task SetProgressAsync(double progress);
		Task SetStatusAsync(BatchStatus status);
		Task SetTransferredItemsCountAsync(int transferredItemsCount);
		Task SetTaggedItemsCountAsync(int taggedItemsCount);
		Task SetMetadataBytesTransferredAsync(long metadataBytesTransferred);
		Task SetFilesBytesTransferredAsync(long filesBytesTransferred);
		Task SetTotalBytesTransferredAsync(long totalBytesTransferred);
		Task SetStartingIndexAsync(int newStartIndex);
	}
}