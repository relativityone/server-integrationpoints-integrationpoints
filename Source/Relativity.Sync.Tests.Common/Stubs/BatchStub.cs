using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Common.Stubs
{
	internal sealed class BatchStub : IBatch
	{
		public BatchStub(int artifactId, int totalItemsCount, int startingIndex)
		{
			ArtifactId = artifactId;
			TotalItemsCount = totalItemsCount;
			StartingIndex = startingIndex;
		}

		public int ArtifactId { get; }

		public int TotalItemsCount { get; }

		public int StartingIndex { get; set; }

		public int FailedItemsCount { get; internal set; }

		public int TransferredItemsCount { get; internal set; }

		public string LockedBy { get; private set; }

		public double Progress { get; private set; }

		public BatchStatus Status { get; private set; }

		public int TaggedItemsCount { get; private set; }

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

		public Task SetTaggedItemsCountAsync(int taggedItemsCount)
		{
			TaggedItemsCount = taggedItemsCount;
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
