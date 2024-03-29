﻿using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
    internal interface IBatch
    {
        int ArtifactId { get; }

        int StartingIndex { get; }

        BatchStatus Status { get; }

        Guid ExportRunId { get; }

        Guid BatchGuid { get; }

        int TransferredItemsCount { get; }

        int FailedItemsCount { get; }

        int TotalDocumentsCount { get; }

        int TransferredDocumentsCount { get; }

        int FailedDocumentsCount { get; }

        long MetadataBytesTransferred { get; }

        long FilesBytesTransferred { get; }

        long TotalBytesTransferred { get; }

        int FailedReadDocumentsCount { get; }

        int ReadDocumentsCount { get; }

        int TaggedDocumentsCount { get; }

        bool IsFinished { get; }

        Task SetTransferredItemsCountAsync(int transferredItemsCount);

        Task SetFailedItemsCountAsync(int failedItemsCount);

        Task SetTransferredDocumentsCountAsync(int transferredDocumentsCount);

        Task SetFailedDocumentsCountAsync(int failedDocumentsCount);

        Task SetMetadataBytesTransferredAsync(long metadataBytesTransferred);

        Task SetFilesBytesTransferredAsync(long filesBytesTransferred);

        Task SetTotalBytesTransferredAsync(long totalBytesTransferred);

        Task SetStatusAsync(BatchStatus status);

        Task SetTaggedDocumentsCountAsync(int taggedDocumentsCount);

        Task SetStartingIndexAsync(int newStartIndex);

        Task SetFailedReadDocumentsCount(int failedReadDocumentsCount);

        Task SetReadDocumentsCount(int readDocumentsCount);
    }
}
