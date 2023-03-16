using System.ComponentModel;
using Banzai;
using Relativity.Sync.Progress;

namespace Relativity.Sync.Extensions
{
    internal static class EnumConversionExtensions
    {
        public static BatchStatus ToBatchStatus(this ExecutionStatus executionStatus)
        {
            BatchStatus batchStatus = BatchStatus.New;
            switch (executionStatus)
            {
                case ExecutionStatus.Canceled:
                    batchStatus = BatchStatus.Cancelled;
                    break;
                case ExecutionStatus.Completed:
                    batchStatus = BatchStatus.Completed;
                    break;
                case ExecutionStatus.CompletedWithErrors:
                    batchStatus = BatchStatus.CompletedWithErrors;
                    break;
                case ExecutionStatus.Failed:
                    batchStatus = BatchStatus.Failed;
                    break;

                case ExecutionStatus.Paused:
                    batchStatus = BatchStatus.Paused;
                    break;
                case ExecutionStatus.Skipped:
                case ExecutionStatus.None:
                    batchStatus = BatchStatus.New;
                    break;
            }

            return batchStatus;
        }

        public static JobHistoryStatus ToJobHistoryStatus(this NodeResultStatus nodeStatus)
        {
            switch (nodeStatus)
            {
                case NodeResultStatus.Succeeded:
                    return JobHistoryStatus.Completed;
                case NodeResultStatus.SucceededWithErrors:
                    return JobHistoryStatus.CompletedWithErrors;
                case NodeResultStatus.Failed:
                    return JobHistoryStatus.Failed;
                default:
                    throw new InvalidEnumArgumentException($"NodeResultStatus - {nodeStatus} is invalid. Contact with Relatity Support.");
            }
        }
    }
}
