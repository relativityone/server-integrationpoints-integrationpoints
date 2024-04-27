using System;
using Relativity.Sync.Extensions;

namespace Relativity.Sync.Kepler.SyncBatch
{
    internal class SyncBatchDto
    {
        public int ArtifactId { get; set; }

        public int WorkspaceArtifactId { get; set; }

        public int TotalDocumentsCount { get; set; }

        public int TransferredDocumentsCount { get; set; }

        public int FailedDocumentsCount { get; set; }

        public Guid ExportRunId { get; set; }

        public Guid BatchGuid { get; set; }

        public int TransferredItemsCount { get; set; }

        public int FailedItemsCount { get; set; }

        public long MetadataBytesTransferred { get; set; }

        public long FilesBytesTransferred { get; set; }

        public long TotalBytesTransferred { get; set; }

        public int TaggedDocumentsCount { get; set; }

        public int FailedReadDocumentsCount { get; set; }

        public int ReadDocumentsCount { get; set; }

        public int InitialStartingIndex { get; set; }

        public int StartingIndex { get; set; }

        public BatchStatus Status { get; set; }

        public bool IsFinished => Status.IsIn(BatchStatus.Completed, BatchStatus.CompletedWithErrors, BatchStatus.Cancelled, BatchStatus.Failed);
    }
}