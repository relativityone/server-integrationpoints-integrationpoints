using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IBatch
	{
		int ArtifactId { get; }
		int FailedItemsCount { get; }
		int TransferredItemsCount { get; }
		int TotalItemsCount { get; }
		int StartingIndex { get; }
		string LockedBy { get; }
		double Progress { get; }
		string Status { get; }
		Task SetFailedItemsCountAsync(int failedItemsCount);
		Task SetTransferredItemsCountAsync(int transferredItemsCount);
		Task SetLockedByAsync(string lockedBy);
		Task SetProgressAsync(double progress);
		Task SetStatusAsync(string status);
	}
}