using System;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.System.Stubs
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

		public int StartingIndex { get; }

		public int FailedItemsCount { get; private set; }

		public int TransferredItemsCount { get; private set; }

		public string LockedBy { get; private set; }

		public double Progress { get; private set; }

		public BatchStatus Status { get; private set; }

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

		public Task SetTransferredItemsCountAsync(int transferredItemsCount)
		{
			TransferredItemsCount = transferredItemsCount;
			return Task.CompletedTask;
		}
	}
}
