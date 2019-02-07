using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class JobCleanupStepTests : FailingStepsBase<IJobCleanupConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot, source tags, dest tags,
			// data destination init, snapshot partition, sync, saved search,
			// data destination finalization, status consolidation, notification
			const int expectedNumberOfExecutedSteps = 13;
			return expectedNumberOfExecutedSteps;
		}
	}
}