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
			// validation, permissions, storage init, object types, snapshot
			// source workspace tags, destination workspace tags
			const int expectedNumberOfExecutedSteps = 9;
			return expectedNumberOfExecutedSteps;
		}
	}
}