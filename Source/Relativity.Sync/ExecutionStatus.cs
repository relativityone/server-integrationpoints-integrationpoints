using System.ComponentModel;

namespace Relativity.Sync
{
    internal enum ExecutionStatus
    {
        [Description("None")]
        None = 0,

        [Description("Completed")]
        Completed,

        [Description("Completed with Errors")]
        CompletedWithErrors,

        [Description("Canceled")]
        Canceled,

        [Description("Skipped")]
        Skipped,

        [Description("Failed")]
        Failed,
        
        [Description("Paused")]
        Paused
    }
}