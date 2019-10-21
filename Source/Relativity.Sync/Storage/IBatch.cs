using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	/// <summary>
	/// Represents Sync Batch.
	/// </summary>
	public interface IBatch
	{
		/// <summary>
		///  Artifact ID.
		/// </summary>
		int ArtifactId { get; }

		/// <summary>
		/// Number of failed items.
		/// </summary>
		int FailedItemsCount { get; }

		/// <summary>
		/// Locked by identifier.
		/// </summary>
		string LockedBy { get; }

		/// <summary>
		/// Batch progress percentage.
		/// </summary>
		double Progress { get; }

		/// <summary>
		/// Starting index.
		/// </summary>
		int StartingIndex { get; }

		/// <summary>
		/// Batch status.
		/// </summary>
		BatchStatus Status { get; }

		/// <summary>
		/// Number of transferred items.
		/// </summary>
		int TransferredItemsCount { get; }

		/// <summary>
		/// Total items in batch.
		/// </summary>
		int TotalItemsCount { get; }

		/// <summary>
		/// Sets number of failed items.
		/// </summary>
		Task SetFailedItemsCountAsync(int failedItemsCount);

		/// <summary>
		/// Sets locked by.
		/// </summary>
		Task SetLockedByAsync(string lockedBy);

		/// <summary>
		/// Sets progress percentage.
		/// </summary>
		Task SetProgressAsync(double progress);

		/// <summary>
		/// Sets status of the batch.
		/// </summary>
		Task SetStatusAsync(BatchStatus status);

		/// <summary>
		/// Sets number of transferred items.
		/// </summary>
		Task SetTransferredItemsCountAsync(int transferredItemsCount);
	}
}