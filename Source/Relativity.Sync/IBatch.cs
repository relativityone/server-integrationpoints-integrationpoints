using System.Threading.Tasks;

namespace Relativity.Sync
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
		Task SetFailedItemsCount(int failedItemsCount);
		Task SetTransferredItemsCount(int transferredItemsCount);
		Task SetLockedBy(string lockedBy);
		Task SetProgress(double progress);
		Task SetStatus(string status);
	}
}