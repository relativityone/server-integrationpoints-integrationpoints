using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceSavedSearchCreationStepTests : FailingStepsBase<IDestinationWorkspaceSavedSearchCreationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// permissions, validation, object types, snapshot
			// sum reporting, source workspace tags, destination workspace tags, data destination initialization,
			// job status consolidation, notification, job cleanup
			const int expectedNumberOfExecutedSteps = 11;
			return expectedNumberOfExecutedSteps;
		}
	}
}