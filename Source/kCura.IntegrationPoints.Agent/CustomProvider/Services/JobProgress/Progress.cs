using System;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    internal sealed class Progress : IEquatable<Progress>
    {
        public int FailedReadDocumentsCount { get; }

        public int TransferredDocumentsCount { get; }

        public Progress(int failedReadDocumentsCount, int transferredDocumentsCount)
        {
            if (failedReadDocumentsCount < 0 || transferredDocumentsCount < 0)
            {
                throw new ArgumentOutOfRangeException("All Progress parameters should has non negative values. " +
                                                      $"failedReadDocumentsCount - {failedReadDocumentsCount}, " +
                                                      $"transferredDocumentsCount - {transferredDocumentsCount}.");
            }
            
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

            return Equals(FailedReadDocumentsCount, other.FailedReadDocumentsCount) &&
                   Equals(TransferredDocumentsCount, other.TransferredDocumentsCount);
        }

        public override int GetHashCode()
        {
            return FailedReadDocumentsCount.GetHashCode() ^ TransferredDocumentsCount.GetHashCode();
        }

        public static Progress operator +(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedReadDocumentsCount + b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount + b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator -(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedReadDocumentsCount - b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount - b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator *(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedReadDocumentsCount * b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount * b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator /(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedReadDocumentsCount / b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount / b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }

        public static Progress operator %(Progress a, Progress b)
        {
            int failedReadDocumentsCount = a.FailedReadDocumentsCount % b.FailedReadDocumentsCount;
            int transferredDocumentsCount = a.TransferredDocumentsCount % b.TransferredDocumentsCount;

            return new Progress(failedReadDocumentsCount, transferredDocumentsCount);
        }
    }
}