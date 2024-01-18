namespace Relativity.Sync
{
    public enum JobHistoryStatus
    {
        Validating,
        ValidationFailed,
        Processing,
        Completed,
        CompletedWithErrors,
        Failed,
        Stopping,
        Stopped,
        Suspending,
        Suspended
    }
}
