﻿namespace Relativity.Sync.Configuration
{
	internal interface IAutomatedWorkflowTriggerConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }
		string TriggerName { get; }
		ExecutionResult SynchronizationExecutionResult { get; set; }
		string TriggerId { get; }
		string TriggerValue { get; }
	}
}