using System;

namespace Relativity.Sync.Storage
{
    internal interface IJobHistoryError
    {
        int ArtifactId { get; }

        string ErrorMessage { get; }

        ErrorStatus ErrorStatus { get; }

        ErrorType ErrorType { get; }

        int JobHistoryArtifactId { get; }

        string Name { get; }

        string SourceUniqueId { get; }

        string StackTrace { get; }

        DateTime TimestampUtc { get; }
    }
}
