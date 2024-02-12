using System;

namespace Relativity.Sync.Storage
{
    internal sealed class JobHistoryError : IJobHistoryError
    {
        public int ArtifactId { get; }

        public string ErrorMessage { get; }

        public ErrorStatus ErrorStatus { get; }

        public ErrorType ErrorType { get; }

        public int JobHistoryArtifactId { get; }

        public string Name { get; }

        public string SourceUniqueId { get; }

        public string StackTrace { get; }

        public DateTime TimestampUtc { get; }

        public JobHistoryError(
            int artifactId,
            string errorMessage,
            ErrorStatus errorStatus,
            ErrorType errorType,
            int jobHistoryArtifactId,
            string name,
            string sourceUniqueId,
            string stackTrace,
            DateTime timestampUtc)
        {
            ArtifactId = artifactId;
            ErrorMessage = errorMessage;
            ErrorStatus = errorStatus;
            ErrorType = errorType;
            JobHistoryArtifactId = jobHistoryArtifactId;
            Name = name;
            SourceUniqueId = sourceUniqueId;
            StackTrace = stackTrace;
            TimestampUtc = timestampUtc;
        }
    }
}
