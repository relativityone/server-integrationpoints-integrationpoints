namespace Relativity.Sync.Transfer.FileMovementService.Models
{
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.management.datafactory.models.pipelinerun.status?view=azure-dotnet
    // Microsoft.Azure.Management.DataFactory.Models.PipelineRun.Status values.
    // Possible values: Queued, InProgress, Succeeded, Failed, Canceling, Cancelled
    public class RunStatuses
    {
        public const string Queued = "Queued";
        public const string InProgress = "InProgress";
        public const string Succeeded = "Succeeded";
        public const string Failed = "Failed";
        public const string Canceling = "Canceling";
        public const string Cancelled = "Cancelled";
    }
}
