namespace Relativity.Sync
{
    internal enum JobHistoryStatus
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
