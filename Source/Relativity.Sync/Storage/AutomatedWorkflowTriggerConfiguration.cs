using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class AutomatedWorkflowTriggerConfiguration : IAutomatedWorkflowTriggerConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;

		public AutomatedWorkflowTriggerConfiguration(SyncJobParameters syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public string TriggerName => "relativity@sync-complete";
		public ExecutionResult SynchronizationExecutionResult { get; set; } = new ExecutionResult(ExecutionStatus.None, string.Empty, null);
	}
}