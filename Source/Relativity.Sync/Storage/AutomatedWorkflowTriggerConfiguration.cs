using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class AutomatedWorkflowTriggerConfiguration : IAutomatedWorkflowTriggerConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;
		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");

		public AutomatedWorkflowTriggerConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);
		public string TriggerName => _syncJobParameters.TriggerName;
		public ExecutionResult SynchronizationExecutionResult { get; set; } = new ExecutionResult(ExecutionStatus.None, string.Empty, null);
		public string TriggerId => _syncJobParameters.TriggerId;
		public string TriggerValue => _syncJobParameters.TriggerValue;
	}
}