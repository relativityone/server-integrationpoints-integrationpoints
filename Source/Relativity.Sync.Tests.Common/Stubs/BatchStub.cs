using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Common.Stubs
{
	internal sealed class BatchStub : IBatch
	{
		public int ArtifactId { get; set; }

		public int TotalItemsCount { get; set; }

		public int StartingIndex { get; set; }

		public int FailedItemsCount { get; set; }

		public int TransferredItemsCount { get; set; }

		public string LockedBy { get; set; }

		public double Progress { get; set; }

		public BatchStatus Status { get; set; }

		public int TaggedDocumentsCount { get; set; }

		public Task SetFailedItemsCountAsync(int failedItemsCount)
		{
			FailedItemsCount = failedItemsCount;
			return Task.CompletedTask;
		}

		public Task SetLockedByAsync(string lockedBy)
		{
			LockedBy = lockedBy;
			return Task.CompletedTask;
		}

		public Task SetProgressAsync(double progress)
		{
			Progress = progress;
			return Task.CompletedTask;
		}

		public Task SetStatusAsync(BatchStatus status)
		{
			Status = status;
			return Task.CompletedTask;
		}

		public Task SetTaggedItemsCountAsync(int taggedDocumentsCount)
		{
			TaggedDocumentsCount = taggedDocumentsCount;
			return Task.CompletedTask;
		}

		public Task SetStartingIndexAsync(int newStartIndex)
		{
			StartingIndex = newStartIndex;
			return Task.CompletedTask;
		}

		public Task SetTransferredItemsCountAsync(int transferredItemsCount)
		{
			TransferredItemsCount = transferredItemsCount;
			return Task.CompletedTask;
		}
	}
}
