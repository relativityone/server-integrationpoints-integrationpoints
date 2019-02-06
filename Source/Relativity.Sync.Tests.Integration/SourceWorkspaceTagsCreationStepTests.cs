using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class SourceWorkspaceTagsCreationStepTests : FailingStepsBase<ISourceWorkspaceTagsCreationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			executorTypes.Should().Contain(x => x == typeof(IDestinationWorkspaceTagsCreationConfiguration));
			executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, storage init, object types, snapshot, destination workspace tags, data destination init, notification
			const int expectedNumberOfExecutedSteps = 8;
			return expectedNumberOfExecutedSteps;
		}
	}
}