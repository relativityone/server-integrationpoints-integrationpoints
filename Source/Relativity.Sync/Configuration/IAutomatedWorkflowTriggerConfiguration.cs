namespace Relativity.Sync.Configuration
{
    internal interface IAutomatedWorkflowTriggerConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }
        string TriggerName { get; }
        ExecutionResult SynchronizationExecutionResult { get; set; }
        string TriggerId { get; }
        string TriggerValue { get; }
        public int RdoArtifactTypeId { get; }
    }
}