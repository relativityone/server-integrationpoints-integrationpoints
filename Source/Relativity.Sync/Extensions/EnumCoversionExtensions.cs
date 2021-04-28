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
    }
}