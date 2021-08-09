using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceObjectTypesCreationStepTests : FailingStepsBase<IDestinationWorkspaceObjectTypesCreationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
		{
			typeof(IPreValidationConfiguration),
			typeof(IJobStatusConsolidationConfiguration),
			typeof(INotificationConfiguration),
			typeof(IJobCleanupConfiguration),
			typeof(IAutomatedWorkflowTriggerConfiguration)
		};
	}
}