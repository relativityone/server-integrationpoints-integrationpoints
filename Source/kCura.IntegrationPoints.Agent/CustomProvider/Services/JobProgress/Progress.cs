using System;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    internal sealed class Progress : IEquatable<Progress>
    {
        public int ReadDocumentsCount { get; }

        public int FailedReadDocumentsCount { get; }

        public int TransferredDocumentsCount { get; }

        public Progress(int readDocumentsCount, int failedReadDocumentsCount, int transferredDocumentsCount)
        {
            if (readDocumentsCount < 0 || failedReadDocumentsCount < 0 || transferredDocumentsCount < 0)
            {
                throw new ArgumentOutOfRangeException("All Progress parameters should has non negative values. " +
                                                      $"readDocumentsCount - {readDocumentsCount}, " +
                                                      $"failedReadDocumentsCount - {failedReadDocumentsCount}, " +
                                                      $"transferredDocumentsCount - {transferredDocumentsCount}.");
            }

            ReadDocumentsCount = readDocumentsCount;
            TransferredDocumentsCount = transferredDocumentsCount;
            FailedReadDocumentsCount = failedReadDocumentsCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Equals(obj as Progress);
        }

        public bool Equals(Progress other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(ReadDocumentsCount, other.ReadDocumentsCount) &&
                   Equals(FailedReadDocumentsCount, other.FailedReadDocumentsCount) &&
                   Equals(TransferredDocumentsCount, other.TransferredDocumentsCount);
        }

        public override int GetHashCode()
        {
            return ReadDocumentsCount.GetHashCode() ^ FailedReadDocumentsCount.GetHashCode() ^ TransferredDocumentsCount.GetHashCode();
        }

        public static Progress operator +(Progress a, Progress b)
        {
            int readDocumentsCount = a.ReadDocumentsCount + b.ReadDocumentsCount;
            int failedReadDocumentsCount = a.FailedReadDocumentsCount + b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount + b.TransferredDocumentsCount;

            return new Progress(readDocumentsCount, failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator -(Progress a, Progress b)
        {
            int readDocumentsCount = a.ReadDocumentsCount - b.ReadDocumentsCount;
            int failedReadDocumentsCount = a.FailedReadDocumentsCount - b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount - b.TransferredDocumentsCount;

            return new Progress(readDocumentsCount, failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator *(Progress a, Progress b)
        {
            int readDocumentsCount = a.ReadDocumentsCount * b.ReadDocumentsCount;
            int failedReadDocumentsCount = a.FailedReadDocumentsCount * b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount * b.TransferredDocumentsCount;

            return new Progress(readDocumentsCount, failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator /(Progress a, Progress b)
        {
            int readDocumentsCount = a.ReadDocumentsCount / b.ReadDocumentsCount;
            int failedReadDocumentsCount = a.FailedReadDocumentsCount / b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount / b.TransferredDocumentsCount;

            return new Progress(readDocumentsCount, failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator %(Progress a, Progress b)
        {
            int readDocumentsCount = a.ReadDocumentsCount % b.ReadDocumentsCount;
            int failedReadDocumentsCount = a.FailedReadDocumentsCount % b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount % b.TransferredDocumentsCount;

            return new Progress(readDocumentsCount, failedReadDocumentsCount, transferredDocumentsCount);
        }
    }
}