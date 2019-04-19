using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class SnapshotPartitionExecutorTests : FailingStepsBase<ISnapshotPartitionConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot, source tags, dest tags,
			// data destination init, saved search, notification
			const int expectedNumberOfExecutedSteps = 9;
			return expectedNumberOfExecutedSteps;
		}
	}
}