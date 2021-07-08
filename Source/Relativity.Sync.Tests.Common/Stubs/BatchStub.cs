using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Common.Stubs
{
	internal sealed class BatchStub : IBatch
	{
		public int ArtifactId { get; set; }
		
		public int StartingIndex { get; set; }

		public int FailedItemsCount { get; set; }

		public int TotalDocumentsCount { get; set; }

		public int TransferredDocumentsCount { get; set; }

		public int FailedDocumentsCount { get; set; }

		public int TransferredItemsCount { get; set; }
		
		public BatchStatus Status { get; set; }

		public int TaggedDocumentsCount { get; set; }

		public Task SetFailedItemsCountAsync(int failedItemsCount)
		{
			FailedItemsCount = failedItemsCount;
			return Task.CompletedTask;
		}

		public Task SetTransferredDocumentsCountAsync(int transferredDocumentsCount)
		{
			TransferredDocumentsCount = transferredDocumentsCount;
			return Task.CompletedTask;
		}

		public Task SetFailedDocumentsCountAsync(int failedDocumentsCount)
		{
			FailedDocumentsCount = failedDocumentsCount;
			return Task.CompletedTask;
		}

		public Task SetStatusAsync(BatchStatus status)
		{
			Status = status;
			return Task.CompletedTask;
		}

		public Task SetTaggedDocumentsCountAsync(int taggedDocumentsCount)
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
