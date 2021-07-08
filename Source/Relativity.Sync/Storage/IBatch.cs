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
		int MetadataBytesTransferred { get; }
		int FilesBytesTransferred { get; }
		int TotalBytesTransferred { get; }
		int TotalItemsCount { get; }

		Task SetFailedItemsCountAsync(int failedItemsCount);
		Task SetLockedByAsync(string lockedBy);
		Task SetProgressAsync(double progress);
		Task SetStatusAsync(BatchStatus status);
		Task SetTransferredItemsCountAsync(int transferredItemsCount);
		Task SetTaggedItemsCountAsync(int taggedItemsCount);
		Task SetMetadataBytesTransferredAsync(int metadataBytesTransferred);
		Task SetFilesBytesTransferredAsync(int filesBytesTransferred);
		Task SetTotalBytesTransferredAsync(int totalBytesTransferred);
		Task SetStartingIndexAsync(int newStartIndex);
	}
}