using System;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    internal sealed class Progress : IEquatable<Progress>
    {
        public int FailedDocumentsCount { get; }

        public int TransferredDocumentsCount { get; }

        public Progress(int failedDocumentsCount, int transferredDocumentsCount)
        {
            if (failedDocumentsCount < 0 || transferredDocumentsCount < 0)
            {
                throw new ArgumentOutOfRangeException("All Progress parameters should has non negative values. " +
                                                      $"failedDocumentsCount - {failedDocumentsCount}, " +
                                                      $"transferredDocumentsCount - {transferredDocumentsCount}.");
            }
            
            TransferredDocumentsCount = transferredDocumentsCount;
            FailedDocumentsCount = failedDocumentsCount;
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

            return Equals(FailedDocumentsCount, other.FailedDocumentsCount) &&
                   Equals(TransferredDocumentsCount, other.TransferredDocumentsCount);
        }

        public override int GetHashCode()
        {
            return FailedDocumentsCount.GetHashCode() ^ TransferredDocumentsCount.GetHashCode();
        }

        public static Progress operator +(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedDocumentsCount + b.FailedDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount + b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator -(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedDocumentsCount - b.FailedDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount - b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator *(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedDocumentsCount * b.FailedDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount * b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator /(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedDocumentsCount / b.FailedDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount / b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator %(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedDocumentsCount % b.FailedDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount % b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }
    }
}