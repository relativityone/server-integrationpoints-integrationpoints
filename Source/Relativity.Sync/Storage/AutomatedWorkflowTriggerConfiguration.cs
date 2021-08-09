using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class AutomatedWorkflowTriggerConfiguration : IAutomatedWorkflowTriggerConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		public AutomatedWorkflowTriggerConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
		public string TriggerName => _syncJobParameters.TriggerName;
		public ExecutionResult SynchronizationExecutionResult { get; set; } = new ExecutionResult(ExecutionStatus.None, string.Empty, null);
		public string TriggerId => _syncJobParameters.TriggerId;
		public string TriggerValue => _syncJobParameters.TriggerValue;
	}
}