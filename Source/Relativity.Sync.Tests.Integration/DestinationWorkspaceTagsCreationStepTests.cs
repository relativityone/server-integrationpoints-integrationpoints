using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceTagsCreationStepTests : FailingStepsBase<IDestinationWorkspaceTagsCreationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			executorTypes.Should().Contain(x => x == typeof(ISourceWorkspaceTagsCreationConfiguration));
			executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot, source workspace tags, data destination init, notification
			const int expectedNumberOfExecutedSteps = 7;
			return expectedNumberOfExecutedSteps;
		}
	}
}